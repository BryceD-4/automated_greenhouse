//This coordinates actions between the Robot and the Crop

using Microsoft.EntityFrameworkCore;
using Greenhouse.Infrastructure.Data;
using Greenhouse.Domain.Models;
using Greenhouse.Domain.Enums;
using Greenhouse.Application.Interfaces;


namespace Greenhouse.Application.Services;

public class TaskService
{
    // private readonly GreenhouseDbContext _dbContext;
    private readonly ITaskRepository _taskRepo;

    // public TaskService(GreenhouseDbContext context)
    public TaskService(ITaskRepository repo)
    {
        // _dbContext = context;
        _taskRepo = repo;
    }

    public async Task CreateWaterTask(int cropId)
    {
        //Search the database for a task that matches this one that is not complete
        //.FirstOrDefault --> Always goes to the database, finds a match using any criteria
        //vs FindAsync -> only searches with primary key
        var existing = await _taskRepo.CheckForCropTask(cropId, TaskType.CropIrrigation);
        
        //If the task exists, we return
        //This prevents duplicates
        if (existing != null) 
        {
            return;
        } 

        var taskPriority = 70;
        //Create the enw task
        var task = new RobotTask
        {
            CropId = cropId,
            Type = TaskType.CropIrrigation,
            Status = TaskState.Pending,
            Progress = 0,
            Duration = 5, 
            Priority = taskPriority
        };
        //Alter the task
        _taskRepo.AddTask(task);
        //Apply these changes to the database via interface
        await _taskRepo.SaveChangesAsync();
    }

    public async Task CreateHarvestTask(int cropId)
    {
        var existing = await _taskRepo.CheckForCropTask(cropId, TaskType.CropHarvesting);

        //Prevent duplicates
        if(existing != null)
        {
            return;
        }

        var taskPriority = 40;

        var task = new RobotTask
        {
          CropId = cropId,
          Type = TaskType.CropHarvesting,
          Status = TaskState.Pending,
          Progress = 0,
          Duration = 10, 
          Priority = taskPriority  
        };

        //Add the task and save changes to the database
        _taskRepo.AddTask(task);
        await _taskRepo.SaveChangesAsync();
    }

    public async Task CreateChargeBatteryTask(int robotId)
    {
        var existing = await _taskRepo.CheckForRobotTask(robotId, TaskType.RobotCharging);
        
        if(existing != null)
        {
            return;
        }

        var taskPriority = 100;

        var task = new RobotTask
        {
            RobotId = robotId,
            Type = TaskType.RobotCharging,
            Status = TaskState.Pending,
            Progress = 0,
            Duration = 15, 
            Priority = taskPriority
        };

        _taskRepo.AddTask(task);
        await _taskRepo.SaveChangesAsync();
    }

    public async Task AssignTask(Robot robot)
    {
        if (robot.CurrentTaskId != null)
        {
            return;
        }
        //Get all tasks with this status, order by descending priority, then by ID
        //Then firstOrDefaultAsync gets the top (first) task from this list
        var task = await _taskRepo.GetNextRobotTask();

        if (task == null) 
        {
            return;
        }
        //Update the task directly only if the task is still pending
        //If it is not pending, then we move on, this avoids two robots taking the same task
        //ExecuteSqlInterpolatedAsync --> sends raw SQL command directly to the database
        var taskAvailability = await _taskRepo.ConfirmTaskIsAvailable(robot.Id, task.Id);

        if (taskAvailability == 0)
        {
            //0== task is already being completed by another robot
            return;
        }

        robot.CurrentTaskId = task.Id;

        await _taskRepo.SaveChangesAsync();

    //OLD METHOD --> this works but is not parallel friendly, all robots pick the same task
        // //If the robot is busy, return
        // if (robot.CurrentTaskId != null)
        // {
        //   return;  
        // } 

        // var task = _dbContext.RobotTasks
        //     .FirstOrDefault(t => t.Status == TaskState.Pending);

        // if (task == null) return;

        // task.Status = TaskState.InProgress;
        // task.RobotId = robot.Id;

        // robot.CurrentTaskId = task.Id;

        // await _dbContext.SaveChangesAsync();
    }

    public async Task ProcessTask(Robot robot, double deltaTime)
    {
        if (robot.CurrentTaskId == null)
        {
            return;
        } 

        //Get the task the robot is working on
        var task = await _taskRepo.GetCurrentRobotTask(robot);

        //If the robot has completed this task
        if(task == null)
        {
            robot.CurrentTaskId = null;
            robot.State = RobotState.Available;
            return;
        }

        //Set the robot status
        //Does not get the crop here as no crop affected by robot charging task
        if(task.Type == TaskType.RobotCharging)
        {
            robot.State = RobotState.Charging;

            //Make dashboard appear as robot is charging
            robot.BatteryLevel += 10;
            if(robot.BatteryLevel > 100)
            {
                robot.BatteryLevel = 100;
            }
        }
        else
        {
            //Only get the crop if it is not a charging task (avoid null pointer)
            var crop = await _taskRepo.GetTargetCrop(task);
            //This can technically return null, so need a safety to avoid compiler warnings
            if(crop != null)
            {
                
                //Set the robot status based on the task
                if(task.Type == TaskType.CropIrrigation)
                {
                    robot.State = RobotState.Irrigating;
                    //Don't let the moisture level reduce further
                    crop.MoistureLevel += 2 * deltaTime;

                } else if(task.Type == TaskType.CropHarvesting)
                {
                    robot.State = RobotState.Harvesting;
                    //Revert so that the health does not change when items are harvested
                    crop.HealthLevel += 1 * deltaTime;
                }
            }
        }

        //Increment the progress value
        task.Progress += deltaTime;

        //If the task is completed, reset everything
        if (task.Progress >= task.Duration)
        {
            //If the robot was charging, the reset and move on
            if(task.Type == TaskType.RobotCharging)
            {
                //Once done charging
                //The robot is at 100%
                robot.BatteryLevel = 100;

            } else
            {
                //Get the crop that was being tended to
                var crop = await _taskRepo.GetTargetCrop(task);
                //This can technically return null, so need a safety to avoid compiler warnings
                if(crop != null){
                    if(task.Type == TaskType.CropIrrigation)
                    {
                        //Moisture level will be full after irrigating
                        crop.MoistureLevel = 100;

                    } else if (task.Type == TaskType.CropHarvesting)
                    {
                        //Once done harvesting, we replant
                        //For now growth is back to 0
                        crop.GrowthLevel = 0;
                        //Set health back to 100%
                        crop.HealthLevel = 100;

                    }
                }
            }
            
            //Mark task as completed
            task.Status = TaskState.Completed;
            //Robot has no more tasks
            robot.CurrentTaskId = null;
            //Robot available for future tasks
            robot.State = RobotState.Available;
        }
        //Set these changes within the database
        await _taskRepo.SaveChangesAsync();
    }
}