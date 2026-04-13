
using Greenhouse.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Infrastructure.Data;

public class GreenhouseDbContext : DbContext
{
    public GreenhouseDbContext(DbContextOptions<GreenhouseDbContext> options) : base(options)
    {
        
    }

    //This runs on first execution, then the model is cached. 
    //if it is changed, it runs on re-execution
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       //this is used to convert the enum values into strings in the database
       //They will initially be trialled as an integer, but this makes them stored as a string

        modelBuilder.Entity<Crop>()
            .Property(c => c.State)
            .HasConversion<string>(); // This tells EF to store the name, not the number
        
        modelBuilder.Entity<Robot>()
            .Property(r => r.State)
            .HasConversion<string>(); 

        //Need one for both enums that RobotTask use (type and status)
        modelBuilder.Entity<RobotTask>()
            .Property(t => t.Status)
            .HasConversion<string>(); 

        modelBuilder.Entity<RobotTask>()
            .Property(t => t.Type)
            .HasConversion<string>(); 
    }

    //these create the tables in the database
    public DbSet<Crop> Crops => Set<Crop>();
    public DbSet<Robot> Robots => Set<Robot>(); 
    public DbSet<RobotTask> RobotTasks => Set<RobotTask>(); 
}