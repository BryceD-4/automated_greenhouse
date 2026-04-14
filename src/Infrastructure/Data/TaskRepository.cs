/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
- This class implements the ITaskRepo interface. 
- methods in this class are called by TaskService
- this class handles all database interactions on behalf of taskService

*/
using Greenhouse.Application.Interfaces;
using Greenhouse.Domain.Models;
using Greenhouse.Domain.Enums;
using Microsoft.EntityFrameworkCore;


namespace Greenhouse.Infrastructure.Data;

public class TaskRepository : ITaskRepository
{
    private readonly GreenhouseDbContext _dbContext;

    public TaskRepository(GreenhouseDbContext context)
    {
        _dbContext = context;
    }

    public async Task<RobotTask?> CheckForCropTask(int cropId, TaskType type)
    {
        //Looking for a task existing with same cropId, not complete, 
        // and has same type 
        //Select highest priority one if there are many, then order by id
        return await _dbContext.RobotTasks
            .Where(t => t.CropId == cropId 
            && t.Status != TaskState.Completed
            && t.Type == type)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.Id)
            .FirstOrDefaultAsync(); 
    }

    //Checks if the robot has a task that is not complete.
    public async Task<RobotTask?> CheckForRobotTask(int robotId, TaskType type)
    {
       //If returning an async task, need await
       //To do this we also need "firstOrDefaultAsync()" to make it truly async
        return await _dbContext.RobotTasks
            .FirstOrDefaultAsync(t => t.RobotId == robotId 
            && t.Status != TaskState.Completed
            && t.Type == type);
    }

    public async Task<RobotTask?> GetNextRobotTask()
    {
        // Get the next highest priority task that is still pending
        return await _dbContext.RobotTasks
            .Where(t => t.Status == TaskState.Pending)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<RobotTask?> GetCurrentRobotTask(Robot robot)
    {
        //Get the task matching the robot's taskId
        return await _dbContext.RobotTasks
            .FirstOrDefaultAsync(t => t.Id == robot.CurrentTaskId);
    }

    //Used to check that a task has not been taking by another robot.
    //ensures parallelization occurs correctly.
    public async Task<int> ConfirmTaskIsAvailable(int robotId, int taskId)
    {
        //REturns an integer, 0 == task is unavailable
        return await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""RobotTasks""
            SET ""Status"" = {"InProgress"},
                ""RobotId"" = {robotId}
            WHERE ""Id"" = {taskId}
            AND ""Status"" = {"Pending"}
        ");
    }

    //Must be nullable Crop with FirstOrDefaultAsync
    public async Task<Crop?> GetTargetCrop(RobotTask task)
    {
        //Get the crop mathcing the indicated ID
        return await _dbContext.Crops.FirstOrDefaultAsync(c => c.Id == task.CropId);
    }

    //Add a task to the RobotTask Database
    public void AddTask(RobotTask task)
    {
        _dbContext.RobotTasks.Add(task);          
    }

    //submit all tracked changes by EF core to the database
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}