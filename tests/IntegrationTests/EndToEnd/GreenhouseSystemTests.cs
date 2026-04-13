/*
    This class holds end to end testing for the entire greenhouse application
    Performs 3 tests:
    1. Dry crop, creates task, robot gets this task, DB updates
    2. Crop ready, harvest task created, robot gets task, DB updates
    3. Robot battery, charge task, robot begins charging, DB updates
*/

using Xunit;
using FluentAssertions;
using Greenhouse.Tests.SharedFiles;
using Greenhouse.Infrastructure.Data;
using Greenhouse.Application.Services;
using Greenhouse.Domain.Enums;
using Greenhouse.Domain.Systems;
using Greenhouse.Domain.Models;
using Microsoft.EntityFrameworkCore;
//Required for IServiceScopeFactory
using Microsoft.Extensions.DependencyInjection;

namespace Greenhouse.Tests.IntegrationTests.Repositories;

public class GreenhouseSystemTests : IClassFixture<PostgresFixture>
{
    private readonly GreenhouseDbContext _context = null!;
    //Needed to run parallel calls
    private readonly IServiceScopeFactory _scopeFactory;

    public GreenhouseSystemTests(PostgresFixture fixture)
    {
        _context = fixture.Context;
        _scopeFactory = fixture.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task EndToEnd_TestDryCropTask_RobotGetsTask()
    {
        //Reset DB before running any tests
        await DbCleaner.Clear(_context);

        //ARRANGE

        var moistureLevel = 20;

        //Crop will stimulate a create water task as low moisture
        var crop = new Crop
        {
            MoistureLevel = moistureLevel,
            State = CropState.Growing
        };

        //Robot does not need to charge, has no task, and is available
        var robot = new Robot
        {
            BatteryLevel = 100,
            CurrentTaskId = null,
            State = RobotState.Available    
        };

        //add these objects into the database 
        _context.Crops.Add(crop);
        _context.Robots.Add(robot);
        //Write these "adds" to the database
        await _context.SaveChangesAsync();

        var repo = new TaskRepository(_context);
        var taskService = new TaskService(repo);
        var ruleSystem = new RuleSystem();
        double deltaTime = 1;

        //ACT
        //Simulate the background services loop
        var crops = _context.Crops.ToList();
        await ruleSystem.EvaluateCrop(crops[0], taskService, deltaTime); 

        //Simulate the "ProcessRobot" loop in background services
        var robots = _context.Robots.ToList();
        var selectedRobot = robots[0];
        await ruleSystem.EvaluateRobot(selectedRobot, taskService);
        await taskService.AssignTask(selectedRobot);
        await taskService.ProcessTask(selectedRobot, deltaTime);  
        //Write these changed to the database
        await _context.SaveChangesAsync();     


        //ASSERT

        var tasks = _context.RobotTasks.ToList();
        //Only should be one task created
        tasks.Count.Should().Be(1);
        //This task should be an irrigaiton task
        tasks[0].Type.Should().Be(TaskType.CropIrrigation);
        //this should be for our crop selected above
        tasks[0].CropId.Should().Be(crop.Id);
        //The progress should have incremented by delatTime
        tasks[0].Progress.Should().Be(deltaTime);

        var changedRobot = _context.Robots.ToList()[0];
        //the robot should now have a task Id matching the above task
        changedRobot.CurrentTaskId.Should().Be(tasks[0].Id);
        //The robots state should be for irrigation
        changedRobot.State.Should().Be(RobotState.Irrigating);

        var changedCrop = _context.Crops.ToList()[0];
        //the crop's moisture level should have incremented
        changedCrop.MoistureLevel.Should().Be(moistureLevel+2);

    }
}
