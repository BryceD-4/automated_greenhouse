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
//This was only for a local connection
// builder.Services.AddDbContext<GreenhouseDbContext>(options =>
//     options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Get the Program.cs to support both local and cloud
// var connectionString =
//     Environment.GetEnvironmentVariable("DATABASE_URL")
//     ?? builder.Configuration.GetConnectionString("DefaultConnection");
    
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;
//If we are using cloud connection, else use a local connection
if (!string.IsNullOrEmpty(databaseUrl))
{
    // This gets to URI from Render
    var uri = new Uri(databaseUrl);
    //Split the key-value pairs into 2 items
    //"user : password" --> ["user", "password"]
    //Now can get user name and password for conection string below
    var userInfo = uri.UserInfo.Split(':');

    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";

} else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

//Use the local or cloud connection described above
builder.Services.AddDbContext<GreenhouseDbContext>(options =>
    options.UseNpgsql(connectionString));

// To align with Dockerfile, get app to listen on port 8080
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

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
