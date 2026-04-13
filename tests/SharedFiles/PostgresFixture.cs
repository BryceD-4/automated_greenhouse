/*
    This class sets up everything that tests need to run before hand and disposes it afterwards. 
    --> start container, create context, this is shared across all tests. 
*/
using Greenhouse.Infrastructure.Data;
using Greenhouse.Domain.Systems;
using Greenhouse.Application.Services;
using Greenhouse.Application.Interfaces;
//REquired to add scope, so that we can run parallel tests
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Tests.SharedFiles;

public class PostgresFixture : IAsyncLifetime
{
    //Use null forgiveness '!' to avoid warnings
    public PostgresTestContainer Container { get; private set; } = null!;
    public GreenhouseDbContext Context { get; private set; } = null!;
    //Required to add scopes to tests when running parallel tests
    public IServiceProvider ServiceProvider { get; private set; } = null!;

    //Create and start the container and context
    public async Task InitializeAsync()
    {
        Container = new PostgresTestContainer();
        await Container.StartAsync();

        Context = DbContextFactory.Create(Container.ConnectionString);

        //Setup the service provider to allow tests to get one scope per thread
        //during parallel execution (giving one context per thread)
        //This way testing acts similarly to the parallel calls in SimulationService.cs
        var services = new ServiceCollection();

        services.AddDbContext<GreenhouseDbContext>(options =>
            options.UseNpgsql(Container.ConnectionString));

        // Register services
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<TaskService>();
        services.AddScoped<RuleSystem>(); 

        ServiceProvider = services.BuildServiceProvider();

    }

    //Dispose of the container and subsequent context. 
    public async Task DisposeAsync()
    {
        await Container.StopAsync();
    }
}