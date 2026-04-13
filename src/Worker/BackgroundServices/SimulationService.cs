//This updates crops, changes moisture level, marks ready for harvest
using Greenhouse.Infrastructure.Data;
using Greenhouse.Domain.DTOs;
using Greenhouse.Domain.Models;
using Greenhouse.Domain.Systems;
using Greenhouse.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Worker.BackgroundServices;

//Background Service = built in .NET feature
//It tells system: to run code continuously in the background during app running
public class SimulationService : BackgroundService
{
    //IServiceScopeFactory --> 
    //GreenhouseDbContext is scoped = does. not live forever, created per request
    //BUT simulation service runs forever (not scoped) = cannot inject DbContext into it
    //SOLUTION --> create a new scope every loop

    //Scope is a dorm of dependency injection
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SimulationService> _logger;

    public SimulationService(IServiceScopeFactory scopeFactory, ILogger<SimulationService> logger)
    {
        _scopeFactory =scopeFactory;
        //Logger is to print out the output
        _logger = logger;
    }

    //This is the method that runs forever "the loop"
    //CancellationToken --> put simply, it tells the loop when to stop
    //i.e. when the app stops (ctrl+c), ".IsCancellationRequested" becomes true
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //create one scope per instance, allows us to get our context
            using var scope = _scopeFactory.CreateScope();
            //Context is the model of the database we use
            //scope.ServiceProvider --> simply allows us to get services in our scope
            var context = scope.ServiceProvider.GetRequiredService<GreenhouseDbContext>();

            //Get the robots and crops from the list within the database
            var robots = await context.Robots.ToListAsync();
            var crops = await context.Crops.ToListAsync();

            //Each loop is one second
            double deltaTime = 1;

            //update the system components to imitate the conditions
            foreach (var crop in crops)
            {
                crop.MoistureLevel -= 1 * deltaTime;
                crop.GrowthLevel += 10 * deltaTime;
            }

            foreach (var robot in robots)
            {
                robot.BatteryLevel -= 5 * deltaTime;
            }

            //Save changes to the database
            await context.SaveChangesAsync();

           //Run parallel processing
           //This is the helper function below
           await RunParallel(crops, crop => ProcessCrop(crop, deltaTime));
           await RunParallel(robots, robot => ProcessRobot(robot, deltaTime));

            //Only run every second
           await Task.Delay(1000, stoppingToken);


    // ____________TRIAL 1: Sequential task handling________

//THIS Version ran all calls sequentially due to the foreach loops and "await" calls
            // using var scope = _scopeFactory.CreateScope();
            // //Create a new scope every loop (as context is scoped, but each loop is one scope)
            // var context = scope.ServiceProvider.GetRequiredService<GreenhouseDbContext>();
            // //Get fresh service every loop
            // var taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
            // var ruleSystem = scope.ServiceProvider.GetRequiredService<RuleSystem>();

            // //1 second per loop
            // double deltaTime = 1;

            // //Get all of the data from the tables
            // var robots = await context.Robots.ToListAsync();
            // var crops = await context.Crops.ToListAsync();
            // //Update crops
            // foreach (var crop in crops)
            // {
            //     //1. Crop losing water over time
            //     crop.MoistureLevel -= 1 * deltaTime;
            //     crop.GrowthLevel += 10 * deltaTime;   
            // }

            // foreach (var robot in robots)
            // {
            //     robot.BatteryLevel -= 5 * deltaTime;
            // }
            
            // //2. Evaluate crop status
            // foreach (var crop in crops)
            // {
            //     await ruleSystem.EvaluateCrop(crop, taskService);
            // }
            // foreach (var robot in robots)
            // {
            //     await ruleSystem.EvaluateRobot(robot, taskService);
            // }

            // //3. Assign task to robot if it is idle
            // foreach(var robot in robots)
            // {
            //     await taskService.AssignTask(robot);
            // }

            // //4. Process task 
            // foreach(var robot in robots) {
            //     await taskService.ProcessTask(robot, deltaTime);
            // }

            // //Save changes to database
            // await context.SaveChangesAsync();

            // //Loop runs every second
            // await Task.Delay(1000, stoppingToken);

            // //To visibly see the changes as they occur
            // // Console.WriteLine($"Water Level: {crop.MoistureLevel}, Task: {robot.CurrentTaskId}");
            // // _logger.LogInformation($"Moisture: {crop.MoistureLevel}, Growth: {crop.GrowthLevel} , Robot: {robot.State}");
        }
    }
    //Parallel Helper method
    //Use "async" to allow for use of "await" within function
    //Task = a single action return type, so it is not a single value but rather provides a handle to track the work execution
    //<T> = generic type = it can be any data type
    //IEnumerable = a collection of items that we want to process
    //Func<T, Task> --> references a method, which takes T-type item input, and returns a Task
    private async Task RunParallel<T>(IEnumerable<T> items, Func<T, Task> action)
    {
        //Limit amount of concurrency to 10 (as to not overwhelm database)
        var semaphore = new SemaphoreSlim(10);
        //Select each item
        var tasks = items.Select(async item =>
        {
            //Before a task can run, it must ask semaphore for permission
            //Only allows 10 to run, then must wait until one finishes
            //i.e. #11 would have to pause
           await semaphore.WaitAsync();
           try
            {
                //Does the work of the function (i.e. Process crop or robot)
                await action(item);
            }
            finally
            {
                //Leave once complete
                semaphore.Release();
            }
        });
        //Wait here until all tasks are finished their work
        await Task.WhenAll(tasks);
    }

    //Crop processsing
    private async Task ProcessCrop(Crop crop, double deltaTime)
    {
        // Same as above, need an individual scope per action
        //As dbContext is not thread safe
        //i.e. if multiple actions try to use the samecontext to alter the database, the data can become unpredictable
        using var scope = _scopeFactory.CreateScope();
        //Get access to the items we need using this particular scope
        var context = scope.ServiceProvider.GetRequiredService<GreenhouseDbContext>();
        var taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
        var ruleSystem = scope.ServiceProvider.GetRequiredService<RuleSystem>();

        //FindAsync = retrieve the item using the primary key == id
        var dbCrop = await context.Crops.FindAsync(crop.Id);

        if(dbCrop == null)
        {
            return;
        }
        //Pass the items from this scope to the system
        await ruleSystem.EvaluateCrop(dbCrop, taskService, deltaTime);
        //Save these changes to the database
        await context.SaveChangesAsync();
    }

    //Robot Processing
    //Same concepts as process crops above
    private async Task ProcessRobot(Robot robot, double deltaTime)
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<GreenhouseDbContext>();
        var taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
        var ruleSystem = scope.ServiceProvider.GetRequiredService<RuleSystem>();

        var dbRobot = await context.Robots.FindAsync(robot.Id);

        if(dbRobot == null)
        {
            return;
        }

        await ruleSystem.EvaluateRobot(dbRobot, taskService);
        await taskService.AssignTask(dbRobot);
        await taskService.ProcessTask(dbRobot, deltaTime);

        await context.SaveChangesAsync();
    }
}