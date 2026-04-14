/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
    - This class tests the proper functioning of code within TaskService.cs
    - Unit tests utilize mock setups for calls to taskRepo to isolate the unit tests. 
*/
using Xunit;
//Used to create mocked version of function calls to TaskRepo
using Moq;
using FluentAssertions;
using Greenhouse.Application.Services;
using Greenhouse.Domain.Enums;
using Greenhouse.Domain.Models;
using Greenhouse.Application.Interfaces;
using System.Data.Common;
using System.Net.Cache;

namespace Greenhouse.Tests.UnitTests.Services;

public class TaskServiceTest
{
    [Fact]
    public async Task CreateWaterTask_TestDuplicateTask()
    {
        int cropId = 1;
        RobotTask returnedTask = new RobotTask();
        var mockRepo = new Mock<ITaskRepository>();

        //Mock the task already existing in the database
        //The method should simply return from here
        mockRepo.Setup(r => r.CheckForCropTask(cropId, TaskType.CropIrrigation))
            .ReturnsAsync(returnedTask);        

        var service = new TaskService(mockRepo.Object);
        await service.CreateWaterTask(cropId);

        //Assert that the rest of the methods in the function were not called
        mockRepo.Verify(r => r.AddTask(It.IsAny<RobotTask>()), Times.Never);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateWaterTask_TestTaskCreation()
    {
        int cropId = 1;
        //Remove warning by adding nullable attribute with '?'
        RobotTask? nullTask = null;

        var mockRepo = new Mock<ITaskRepository>();

        //Mock that the task does not exist, need to "setup" this return type
        mockRepo.Setup(repo => repo.CheckForCropTask(cropId, TaskType.CropIrrigation))
            .ReturnsAsync(nullTask);

        //Mock the save changes code running
         mockRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        //Call method
        var service = new TaskService(mockRepo.Object);

        await service.CreateWaterTask(cropId);

        //Do not need to setup for add task since it is a void method
        //We just need to verify it ran with the correct method
        //Make sure the correct object was passed through this method once
        mockRepo.Verify(repo => repo.AddTask(
            It.Is<RobotTask>( t => 
            t.CropId == cropId && 
            t.Type == TaskType.CropIrrigation && 
            t.Status == TaskState.Pending &&
            t.Progress == 0 &&
            t.Duration == 5 &&
            t.Priority == 70
            //Make sure this runs only once
            )), Times.Once);

        //Make sure saveChangesAsync() runs at least/only once
        mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateHarvestTask_TestTaskAlreadyExists()
    {
        int cropId = 1;
        RobotTask returnedTask = new RobotTask();

        var mockRepo = new Mock<ITaskRepository>();

        mockRepo.Setup(repo => repo.CheckForCropTask(cropId, TaskType.CropHarvesting))
            .ReturnsAsync(returnedTask);

        var service = new TaskService(mockRepo.Object);
        await service.CreateHarvestTask(cropId);

        //Verify that the methods proceeding did not run
        mockRepo.Verify(repo => repo.AddTask(It.IsAny<RobotTask>()), Times.Never);
        mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateHarvestTask_TestTaskCreation()
    {
        int cropId = 1;
        RobotTask? nullTask = null;

        var mockRepo = new Mock<ITaskRepository>();

        //Mock the task not already existing
        mockRepo.Setup(repo => repo.CheckForCropTask(cropId, TaskType.CropHarvesting))
            .ReturnsAsync(nullTask);

        //Mock the save changes code running
         mockRepo.Setup(rep => rep.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        
        var service = new TaskService(mockRepo.Object);
        await service.CreateHarvestTask(cropId);

        //Make sure the correct object was passed through this method once
        mockRepo.Verify(repo => repo.AddTask(
            It.Is<RobotTask>( t => 
            t.CropId == cropId && 
            t.Type == TaskType.CropHarvesting && 
            t.Status == TaskState.Pending &&
            t.Progress == 0 &&
            t.Duration == 10 &&
            t.Priority == 40
            //Make sure this runs only once
            )), Times.Once);

        //Make sure saveChangesAsync() runs at least/only once
        mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

[Fact]
    public async Task CreateChargeBatteryTask_TestTaskAlreadyExists()
    {
        int robotId = 1;
        RobotTask returnedTask = new RobotTask();

        var mockRepo = new Mock<ITaskRepository>();

        mockRepo.Setup(repo => repo.CheckForRobotTask(robotId, TaskType.RobotCharging))
            //Need returnsAsync to return a Task<> from the method
            //wraps the returned object in a Task<>
            .ReturnsAsync(returnedTask);

        var service = new TaskService(mockRepo.Object);
        await service.CreateChargeBatteryTask(robotId);

        //Verify that the methods proceeding did not run
        mockRepo.Verify(repo => repo.AddTask(It.IsAny<RobotTask>()), Times.Never);
        mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateChargeBatteryTask_TestTaskCreation()
    {
        int robotId = 1;
        RobotTask? nullTask = null;

        var mockRepo = new Mock<ITaskRepository>();

        //Mock the task not already existing
        mockRepo.Setup(repo => repo.CheckForRobotTask(robotId, TaskType.RobotCharging))
            .ReturnsAsync(nullTask);

        //Mock the save changes code running, and returns that run correctly
         mockRepo.Setup(rep => rep.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        
        var service = new TaskService(mockRepo.Object);
        await service.CreateChargeBatteryTask(robotId);

        //Make sure the correct object was passed through this method once
        mockRepo.Verify(repo => repo.AddTask(
            It.Is<RobotTask>( t => 
            t.RobotId == robotId && 
            t.Type == TaskType.RobotCharging && 
            t.Status == TaskState.Pending &&
            t.Progress == 0 &&
            t.Duration == 15 &&
            t.Priority == 100
            //Make sure this runs only once
            )), Times.Once);

        //Make sure saveChangesAsync() runs at least/only once
        mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }



    //The assign task should return if robot is already assigned a task
    [Fact]
    public async Task AssignTask_TestIfRobotAlreadyAssigned()
    {
        var robot = new Robot
        {
          Id = 1,
          Name = "Robo1",
          BatteryLevel = 15,
          //Already has a task assigned
          CurrentTaskId = 1,
          State = RobotState.Harvesting
        };

        var mockRepo = new Mock<ITaskRepository>();
        
        var service = new TaskService(mockRepo.Object);

        await service.AssignTask(robot);

        robot.CurrentTaskId.Should().Be(1);
    }

    [Fact]
    public async Task AssignTask_TestIfNoTasks()
    {
        var robot = new Robot
        {
          Id = 1,
          Name = "Robo1",
          BatteryLevel = 15,
          //Does not have a task assigned
          CurrentTaskId = null,
          State = RobotState.Harvesting
        };

        RobotTask? nullTask = null;
        
        //Mock the interface
        var mockRepo = new Mock<ITaskRepository>();
        //Mock that there are no tasks left to run (returns null)
        mockRepo.Setup(repo => repo.GetNextRobotTask())
            .ReturnsAsync(nullTask);
        
        var service = new TaskService(mockRepo.Object);

        await service.AssignTask(robot);
        //Confirm no task was assigned
        robot.CurrentTaskId.Should().BeNull();

        //Apply a random taskId to mock the method running
        int taskId = 2;

        //Confirm the methods did not run
        mockRepo.Verify(repo => repo.ConfirmTaskIsAvailable(robot.Id, taskId), Times.Never);
        mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

     //Test that race conditions are avoided during task assginment
    [Fact]
    public async Task AssignTask_TestIfTaskUnavailable()
    {
        var robot = new Robot
        {
            Id = 1,
            Name = "Robo1",
            BatteryLevel = 15,
            CurrentTaskId = null,
            State = RobotState.Available
        };

        RobotTask returnedTask = new RobotTask {
            Id = 3, Type = TaskType.CropHarvesting, 
            RobotId = 1, CropId = 2, Status = TaskState.InProgress, Progress = 0, Duration = 10, 
            Priority = 40
        };

        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(r => r.GetNextRobotTask())
            .ReturnsAsync(returnedTask);
        
        //Simulate race condition, returning 0 == task is not available
        mockRepo.Setup(r => r.ConfirmTaskIsAvailable(robot.Id,returnedTask.Id))
            .ReturnsAsync(0);

        var service = new TaskService(mockRepo.Object);

        await service.AssignTask(robot);
        robot.CurrentTaskId.Should().BeNull();
        //Make sure last emthod did not run
        mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
    
    //Ensure robot task is assigned correctly
    [Fact]
    public async Task AssignTask_TestPickNextTask_HighestPriority()
    {
       var robot = new Robot
        {
          Id = 1,
          Name = "Robo1",
          BatteryLevel = 15,
          CurrentTaskId = null,
          State = RobotState.Available
        };
        var expectedTask = new RobotTask {
            Id = 3, Type = TaskType.RobotCharging, 
            RobotId = 1, CropId = null, Status = TaskState.Pending, Progress = 0, Duration = 10, 
            Priority = 40
        };
       var mockRepo = new Mock<ITaskRepository>();
       mockRepo.Setup(r => r.GetNextRobotTask())
            .ReturnsAsync(expectedTask);

        //Mock all the calls within AssignTask
        //Return 1 = task is available
        mockRepo.Setup(r => r.ConfirmTaskIsAvailable(robot.Id, expectedTask.Id))
            .ReturnsAsync(1);
        mockRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        //Needs DB context
        var service = new TaskService(mockRepo.Object);

        await service.AssignTask(robot);

        //It should come back with an Id of 3
        robot.CurrentTaskId.Should().Be(3);

        //Option to not use fluent assertions
        // Assert.Equal(robot.CurrentTaskId, 3);

    }

    [Fact]
    public async Task ProcessTask_TestIfRobotHasNoTask()
    {
        Robot robot = new Robot{CurrentTaskId = null};
        double deltaTime = 1;
        RobotTask task = new RobotTask{};

        var mockRepo = new Mock<ITaskRepository>();
        var service = new TaskService(mockRepo.Object);
        await service.ProcessTask(robot, deltaTime);
        //Ensure the other methods in the function did not run
        mockRepo.Verify(rep => rep.GetCurrentRobotTask(robot), Times.Never);
        mockRepo.Verify(rep => rep.GetTargetCrop(task), Times.Never);
        mockRepo.Verify(rep => rep.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ProcessTask_TestIfCurrentTaskIsNull()
    {
        //Set robot to the state that should be changed
        Robot robot = new Robot{
            CurrentTaskId = 1, 
            State = RobotState.Harvesting
        };

        double deltaTime = 1;
        RobotTask? taskReturned = null;
        RobotTask task = new RobotTask{};

        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(repo => repo.GetCurrentRobotTask(robot))
            .ReturnsAsync(taskReturned);

        var service = new TaskService(mockRepo.Object);
        await service.ProcessTask(robot, deltaTime);

        //Make sure robots taskId and State are changed correctly
        robot.CurrentTaskId.Should().BeNull();
        robot.State.Should().Be(RobotState.Available);

        //Make sure any remaining functions did not run
        mockRepo.Verify(rep => rep.GetTargetCrop(task), Times.Never);
        mockRepo.Verify(rep => rep.SaveChangesAsync(), Times.Never);

    }

    [Fact]
    public async Task ProcessTask_TestIfTaskNotDone_RobotCharging()
    {
         //Set robot to the state that should be changed
         //Battery level as well
        Robot robot = new Robot{
            CurrentTaskId = 1, 
            State = RobotState.Available,
            BatteryLevel = 95
        };

        double deltaTime = 1;
        RobotTask task = new RobotTask
        {
            Type = TaskType.RobotCharging,
            Progress = 0,
            Duration = 10
        };

        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(repo => repo.GetCurrentRobotTask(robot))
            .ReturnsAsync(task);

        var service = new TaskService(mockRepo.Object);
        await service.ProcessTask(robot, deltaTime);

        //Make sure robot state changed
        robot.State.Should().Be(RobotState.Charging);
        //Battery Level should be 95+10 = 105, but capped to 100
        robot.BatteryLevel.Should().Be(100);

        //Task progress should be incremented by deltatime
        task.Progress.Should().Be(deltaTime);

        //Other methods should not run in other paths
        mockRepo.Verify(rep => rep.GetTargetCrop(task), Times.Never);

        //This should run once
        mockRepo.Verify(rep => rep.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessTask_TestIfTaskNotDone_Irrigation()
    {
        //Set robot to the state that should be changed
         //Battery level as well
        Robot robot = new Robot{
            CurrentTaskId = 1, 
            State = RobotState.Available,
            BatteryLevel = 95
        };

        double deltaTime = 1;
        RobotTask task = new RobotTask
        {
            Type = TaskType.CropIrrigation,
            CropId = 1,
            Progress = 0,
            Duration = 10
        };

        Crop crop = new Crop
        {
            MoistureLevel = 0
        };

        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(repo => repo.GetCurrentRobotTask(robot))
            .ReturnsAsync(task);

        mockRepo.Setup(repo => repo.GetTargetCrop(task))
            .ReturnsAsync(crop);

        var service = new TaskService(mockRepo.Object);
        await service.ProcessTask(robot, deltaTime);

        //Robot state changes correctly
        robot.State.Should().Be(RobotState.Irrigating);
        //Crop moisture changed
        crop.MoistureLevel.Should().Be(2);
        //Task should increment
        task.Progress.Should().Be(deltaTime);

        //This should run once
        mockRepo.Verify(rep => rep.SaveChangesAsync(), Times.Once);
    }

//Not complete, to do for practice
//     [Fact]
//     public async Task ProcessTask_TestIfTaskNotDone_Harvesting()
//     {
        
//     }

    [Fact]
    public async Task ProcessTask_TestIfTaskDone_Charging()
    {
        //Set robot to the state that should be changed
        //Battery level as well
        Robot robot = new Robot{
            CurrentTaskId = 1, 
            State = RobotState.Available,
            BatteryLevel = 95
        };

        double deltaTime = 1;
        RobotTask task = new RobotTask
        {
            Type = TaskType.RobotCharging,
            CropId = 1,
            Progress = 9,
            Duration = 10
        };

        Crop crop = new Crop
        {
            MoistureLevel = 0
        };

        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(repo => repo.GetCurrentRobotTask(robot))
            .ReturnsAsync(task);  

        var service = new TaskService(mockRepo.Object);
        await service.ProcessTask(robot, deltaTime);

        //Task status should be completed
        task.Status.Should().Be(TaskState.Completed);
        //Make sure progress updated to be equal to duration
        task.Progress.Should().Be(task.Duration);

        //Robot taskId to be null
        robot.CurrentTaskId.Should().BeNull();
        //Make sure robot state is available
        robot.State.Should().Be(RobotState.Available);
        //Battery should be 100% charged
        robot.BatteryLevel.Should().Be(100);

        //Other methods should not run in other paths
        mockRepo.Verify(rep => rep.GetTargetCrop(task), Times.Never);

        //This should run once
        mockRepo.Verify(rep => rep.SaveChangesAsync(), Times.Once);

    }

//To do for practice
//     [Fact]
//     public async Task ProcessTask_TestIfTaskDone_Irrigation()
//     {
        
//     }
   
//    [Fact]
//     public async Task ProcessTask_TestIfTaskDone_Harvesting()
//     {
        
//     }
}
