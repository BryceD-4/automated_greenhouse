/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
- Main driver file of the program for Greenhouse Application. 

*/
using Greenhouse.PresentationAPI.EndPoints;
using Greenhouse.Application.Services;
using Greenhouse.Infrastructure.Data;
using Greenhouse.Worker.BackgroundServices;
using Greenhouse.Domain.Systems;
using Greenhouse.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Use the default connection string found in appsetttings.json
builder.Services.AddDbContext<GreenhouseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
//REgister all services
builder.Services.AddScoped<GreenhouseService>();
builder.Services.AddScoped<CropService>();
builder.Services.AddScoped<RobotService>();
builder.Services.AddScoped<TaskService>();

//Register all infrastructure items
builder.Services.AddScoped<RuleSystem>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

//This lets us activate "Background services" to create a loop
builder.Services.AddHostedService<SimulationService>();

//This is to make output cleaner in the terminal window
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Reduce EF Core logging noise in the terminal
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

//Add all controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//Once everything is registered, build the application
var app = builder.Build();
//Allows for the use of endpoints, call the functions within the end point files
app.MapGreenhouseEndpoints();
app.MapCropEndpoints();
app.MapRobotEndpoints();

//Added to allow html output for the live dashboard
app.UseDefaultFiles();
app.UseStaticFiles();

//For controllers: to allow them to be mapped automatically by the program
app.MapControllers();

//Run the application
app.Run();
