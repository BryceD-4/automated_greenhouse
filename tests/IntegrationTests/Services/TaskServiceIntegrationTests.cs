/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
    - This class tests the TaskService.cs class functionality
    - Tests using a containerized test database to test actual postGreSQL interactions
    via task repository
    - TaskService > TaskRepository > PostgreSQL
*/
using Xunit;
using FluentAssertions;
using Greenhouse.Tests.SharedFiles;
using Greenhouse.Infrastructure.Data;
using Greenhouse.Application.Services;
using Greenhouse.Domain.Enums;
using Microsoft.EntityFrameworkCore;
//Required for IServiceScopeFactory
using Microsoft.Extensions.DependencyInjection;

namespace Greenhouse.Tests.IntegrationTests.Services;

//Fixture creates and starts the test db container
//Any class implementing IClassFixture tells the test runner (xUnit)
//to create an instance of the fixture (<PostgresFixture>)
//Fixture = a set it that needs to be in the environment for it to run
//Fixture = a staple item, cannot go without
public class TaskServiceTests : IClassFixture<PostgresFixture>
{
    private readonly GreenhouseDbContext _context = null!;
    //Needed to run parallel calls
    private readonly IServiceScopeFactory _scopeFactory;


    //Acts as a Setup call
    //xUnit automatically creates an instance of PostgresFixture and passes it to
    //this constructor
    public TaskServiceTests(PostgresFixture fixture)
    {
        _context = fixture.Context;
        _scopeFactory = fixture.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task CreateWaterTask_TestDoesNotCreateDuplicate()
    {
        // Reset DB
        await DbCleaner.Clear(_context);

        // Arrange, get the systems required
        var repo = new TaskRepository(_context);
        var service = new TaskService(repo);

        int cropId = 1;

        // Act, try to create the same task twice
        await service.CreateWaterTask(cropId);
        await service.CreateWaterTask(cropId);

        // Assert, check that a duplicate was not made
        var tasks = _context.RobotTasks
            .Where(t => t.CropId == cropId && t.Type == TaskType.CropIrrigation)
            .ToList();
        //Only one task should be present, no duplicates
        tasks.Count.Should().Be(1);
    }

    [Fact]
    public async Task CreateWaterTask_TestCreateTask()
    {
        // Reset DB to not have carry over from previous tests
        await DbCleaner.Clear(_context);
        //Arrange
        var repo = new TaskRepository(_context);
        var service = new TaskService(repo);

        int cropId = 1;

        //Act
        await service.CreateWaterTask(cropId);

        //Assert
        var tasks = _context.RobotTasks.ToList();

        tasks.Count.Should().Be(1);
        tasks[0].CropId.Should().Be(cropId);
    }



    //Testing concurrency at the task service level
    [Fact]
    public async Task CreateWaterTask_ShouldNotCreateDuplicates_UnderConcurrency()
    {
        await DbCleaner.Clear(_context);
        //ARRANGE the items needed
        var repo = new TaskRepository(_context);
        var service = new TaskService(repo);

        int cropId = 1;

        //ACT: Simulate multiple concurrent requests
        //10 calls in parallel
        //Needs to run the same way as backgroundService runs --> each thread needs its own context
        var tasks = Enumerable.Range(0, 10)
            .Select(async _ =>
            {
                using var scope = _scopeFactory.CreateScope();

                var service = scope.ServiceProvider.GetRequiredService<TaskService>();
                await service.CreateWaterTask(cropId);
            });

        await Task.WhenAll(tasks);

        //ASSERT: Make sure only one task was created
        //verification needs a fresh scope, as only scopes existed were above
        using var verifyScope = _scopeFactory.CreateScope();
        var context = verifyScope.ServiceProvider.GetRequiredService<GreenhouseDbContext>();

        var results = context.RobotTasks
            .Where(t => t.CropId == cropId && t.Type == TaskType.CropIrrigation)
            .ToList();

        results.Count.Should().Be(1);
    }

}