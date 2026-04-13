using Greenhouse.PresentationAPI.EndPoints;
using Greenhouse.Application.Services;
using Greenhouse.Infrastructure.Data;
using Greenhouse.Worker.BackgroundServices;
using Greenhouse.Domain.Systems;
using Greenhouse.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GreenhouseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<GreenhouseService>();
builder.Services.AddScoped<CropService>();
builder.Services.AddScoped<RobotService>();
builder.Services.AddScoped<TaskService>();

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

var app = builder.Build();

app.MapGreenhouseEndpoints();
app.MapCropEndpoints();
app.MapRobotEndpoints();

//Added to allow html output for the live dashboard
app.UseDefaultFiles();
app.UseStaticFiles();

// app.UseRouting();

//For controllers:
app.MapControllers();

app.Run();
