/*

This class tests TaskRepository using an in-memory database.

*/
using Xunit;
using FluentAssertions;
using Greenhouse.Tests.SharedFiles;
using Greenhouse.Domain.Models;
using Greenhouse.Domain.Enums;
using Greenhouse.Infrastructure.Data;

namespace Greenhouse.Tests.IntegrationTests.Repositories;


public class TaskRepositoryTests
{

    //__________CROPTASK
    [Fact]
    public async Task CheckForCropTask_TestNormalConditions()
    {
        var context = DbContextFactory.CreateInMemoryDbContext();
        int cropId = 1;

        context.RobotTasks.AddRange(
            new RobotTask { Id = 1, Type = TaskType.CropIrrigation, CropId = cropId, Status = TaskState.Pending, Priority = 1 },
            new RobotTask { Id = 2, Type = TaskType.CropIrrigation, CropId = cropId, Status = TaskState.Pending, Priority = 5 },
            new RobotTask { Id = 3, Type = TaskType.CropIrrigation, CropId = cropId, Status = TaskState.Completed, Priority = 10 }
        );

        await context.SaveChangesAsync();
        var repo = new TaskRepository(context);
        var result = await repo.CheckForCropTask(cropId, TaskType.CropIrrigation);

        //REsult should not be null, stops warnings of null reference from occurring
        result.Should().NotBeNull();
        //The second task should be the one selected
        result.Id.Should().Be(2);
        //Check it is the matching priority
        result.Priority.Should().Be(5);
    }

    [Fact]
    public async Task CheckForCropTask_TestNoTasks()
    {
        var context = DbContextFactory.CreateInMemoryDbContext();
        int cropId = 1;

        var task_1 = new RobotTask { Id = 1, Type = TaskType.CropIrrigation, CropId = cropId, Status = TaskState.Pending, Priority = 1 };
        var task_2 = new RobotTask { Id = 2, Type = TaskType.CropIrrigation, CropId = cropId, Status = TaskState.Pending, Priority = 5 };
        var task_3 = new RobotTask { Id = 3, Type = TaskType.CropIrrigation, CropId = cropId, Status = TaskState.Completed, Priority = 10 };

        context.RobotTasks.AddRange(
            task_1, task_2, task_3
        );

        await context.SaveChangesAsync();
        var repo = new TaskRepository(context);
        var result = await repo.CheckForCropTask(cropId, TaskType.CropHarvesting);

        //Result should be null, as no tasks have this type
        result.Should().BeNull();
    }

//__________ROBOTTASK
    [Fact]
    public async Task CheckForRobotTask_TestNormalConditions()
    {
        var context = DbContextFactory.CreateInMemoryDbContext();
        int robotId = 1;

        var task_1 = new RobotTask {Type = TaskType.RobotCharging, RobotId = robotId, Status = TaskState.Pending, Priority = 1 };
        var task_2 = new RobotTask {Type = TaskType.CropIrrigation, RobotId = robotId, Status = TaskState.Pending, Priority = 5 };
        var task_3 = new RobotTask {Type = TaskType.RobotCharging, RobotId = robotId, Status = TaskState.Completed, Priority = 10 };

        context.RobotTasks.AddRange(
            task_1, task_2, task_3
        );

        // context.RobotTasks.AddRange(
        //     new RobotTask { Id = 1, Type = TaskType.RobotCharging, RobotId = robotId, Status = TaskState.Pending, Priority = 1 },
        //     new RobotTask { Id = 2, Type = TaskType.CropIrrigation, RobotId = robotId, Status = TaskState.Pending, Priority = 5 },
        //     new RobotTask { Id = 3, Type = TaskType.CropIrrigation, RobotId = robotId, Status = TaskState.Completed, Priority = 10 }
        // );

        await context.SaveChangesAsync();
        var repo = new TaskRepository(context);
        var result = await repo.CheckForRobotTask(robotId, TaskType.RobotCharging);

        //REsult should not be null, stops warnings of null reference from occurring
        result.Should().NotBeNull();
        //Should return the first task listed
        result.Id.Should().Be(task_1.Id);
    }

    [Fact]
    public async Task CheckForRobotTask_TestNoTasks()
    {
        var context = DbContextFactory.CreateInMemoryDbContext();
        int robotId = 1;
        //Prefill the mock database
        context.RobotTasks.AddRange(
            new RobotTask { Id = 1, Type = TaskType.CropIrrigation, RobotId = robotId, Status = TaskState.Pending, Priority = 1 },
            new RobotTask { Id = 2, Type = TaskType.CropIrrigation, RobotId = robotId, Status = TaskState.Pending, Priority = 5 },
            new RobotTask { Id = 3, Type = TaskType.CropIrrigation, RobotId = robotId, Status = TaskState.Completed, Priority = 10 }
        );

        await context.SaveChangesAsync();
        var repo = new TaskRepository(context);
        //NEED await here or else returns the promise and not the actual recovered item
        var result = await repo.CheckForRobotTask(robotId, TaskType.RobotCharging);

        //Result should be null, as no tasks have this type
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetNextRobotTask_Returns_HighestPriorityPendingTask()
    {
        var context = DbContextFactory.CreateInMemoryDbContext();
       
        //populate the mock database
        //AddRange to add multiple
        context.RobotTasks.AddRange(
            new RobotTask { Id = 1, Status = TaskState.Pending, Priority = 1 },
            new RobotTask { Id = 2, Status = TaskState.Pending, Priority = 5 },
            new RobotTask { Id = 3, Status = TaskState.Completed, Priority = 10 }
        );

        await context.SaveChangesAsync();

        var repo = new TaskRepository(context);

        var result = await repo.GetNextRobotTask();

        Assert.NotNull(result);
        Assert.Equal(2, result.Id); // highest priority pending
    }

    [Fact]
    public async Task GetCurrentRobotTask_ReturnsCorrectTask()
    {
        var context = DbContextFactory.CreateInMemoryDbContext();

        var task_1 = new RobotTask {Status = TaskState.Pending, Priority = 1 };
        var task_2 = new RobotTask {Status = TaskState.Pending, Priority = 5 };

        context.RobotTasks.AddRange(
            task_1, task_2
        );
        await context.SaveChangesAsync();

        //Generate the objects needed
        var robot = new Robot { CurrentTaskId = task_1.Id };

        var repo = new TaskRepository(context);
        

        var result = await repo.GetCurrentRobotTask(robot);

        result.Should().NotBeNull();
        //Make sure the correct task was returned
        result.Id.Should().Be(task_1.Id);

        // Assert.NotNull(result);
        // Assert.Equal(10, result.Id);
    }

    //Testing a function with rawSQL
    [Fact]
    public async Task ConfirmTaskIsAvailable_UpdatesTask()
    {
        //Use SqlLite to work with RawSQL mocking
        var context = DbContextFactory.CreateSqlite();

        int robotId = 2;
        int taskId = 1;

        context.RobotTasks.Add(new RobotTask
        {
            Id = taskId,
            Status = TaskState.Pending
        });

        await context.SaveChangesAsync();

        var repo = new TaskRepository(context);

        var rows = await repo.ConfirmTaskIsAvailable(robotId, taskId);

        //There should be 1 row total
        // Assert.Equal(1, rows);
        rows.Should().Be(1);

        var updated = context.RobotTasks.First();
        //Need to reload the entity to see the changes made from call above
        await context.Entry(updated).ReloadAsync(); 

        //Make sure the task was update appropriately
        // Assert.Equal(TaskState.InProgress, updated.Status);
        // Assert.Equal(5, updated.RobotId);
        updated.RobotId.Should().Be(robotId);
        updated.Status.Should().Be(TaskState.InProgress);
    }

    //TODO for practice

    // [Fact]
    // public async Task GetTargetCrop_Test(){}

    // [Fact]
    // public async Task AddTask_Test(){}
}