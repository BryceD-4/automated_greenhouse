/*
   PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
    - Tests TaskRepository.cs
    - This class runs integration tests to test reall interaction with the actual database. 
    - Uses a containerized version of the DB. 
    - i.e. This tests the repository interaction with the real (test) DB. 
*/
using Xunit;
using Greenhouse.Tests.SharedFiles;
using Greenhouse.Infrastructure.Data;
using Greenhouse.Domain.Models;
using Greenhouse.Domain.Enums;
using Greenhouse.Application.Services;

namespace Greenhouse.Tests.IntegrationTests.Database;


using System.Threading.Tasks;

//IClassFixture handles all the database initializatinon and disposing. 
//This allows it to be only initialized and displosed once per class and not once per test. 
public class TaskRepositoryIntegrationTests : IClassFixture<PostgresFixture>
{

    //Add null forgiveness "!" to recognize knowing it is set as null
    private GreenhouseDbContext _context = null!;

    public TaskRepositoryIntegrationTests(PostgresFixture fixture)
    {
        _context = fixture.Context;
    }

    [Fact]
    public async Task GetCurrentRobotTask_ReturnsCorrectTask()
    {
        //clear the database before moving onto the next test
        await DbCleaner.Clear(_context);

        // Arrange
        var task1 = new RobotTask { Status = TaskState.Pending, Priority = 1 };
        var task2 = new RobotTask { Status = TaskState.Pending, Priority = 5 };

        _context.RobotTasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        //Link robot to this specific task
        var robot = new Robot { CurrentTaskId = task1.Id };
        _context.Robots.Add(robot);
        await _context.SaveChangesAsync();

        var repo = new TaskRepository(_context);

        // Act
        //Must be await or else unpredictable return value
        var result = await repo.GetCurrentRobotTask(robot);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task1.Id, result.Id);
    }

    [Fact]
    public async Task CreateWaterTask_DoesNotCreateDuplicate()
    {
        //clear the database before moving onto the next test
        await DbCleaner.Clear(_context);

        var repo = new TaskRepository(_context);
        var service = new TaskService(repo);

        int cropId = 1;

        // Act
        await service.CreateWaterTask(cropId);
        await service.CreateWaterTask(cropId);

        var tasks = _context.RobotTasks
            .Where(t => t.CropId == cropId && t.Type == TaskType.CropIrrigation)
            .ToList();

        // Assert
        Assert.Single(tasks);
    }
}