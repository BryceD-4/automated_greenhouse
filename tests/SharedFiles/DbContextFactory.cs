/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
    - Used to create a mock database for testing code in TaskRepositoryTests.cs
*/
using Greenhouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace Greenhouse.Tests.SharedFiles;

public static class DbContextFactory
{
    public static GreenhouseDbContext CreateInMemoryDbContext()
    {
        //Build a temporary Database in memory
        var options = new DbContextOptionsBuilder<GreenhouseDbContext>()
            //This tells EF to use a mock in memory database and not the actual database
            //Guid.NewGuid. -> makes sure each test gets a new database, or else all tests share
            //same in memory database = random failures
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
            .Options;

        return new GreenhouseDbContext(options);
    }

    //This is to test SQL queries with raw SQL
    public static GreenhouseDbContext CreateSqlite()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<GreenhouseDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new GreenhouseDbContext(options);

        //With postgreSQL, need EnsureCreated() or Migrate()
        context.Database.EnsureCreated();

        return context;
    }

    //For integration testing with a containerized database
     public static GreenhouseDbContext Create(string connectionString)
    {
        var options = new DbContextOptionsBuilder<GreenhouseDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var context = new GreenhouseDbContext(options);
        
        //ensures the imgration of the data has occured for the database test
        //With postgreSQL, need EnsureCreated() or Migrate()
        //This makes the test DB have all of the tables, columns, primary keys etc
        //The test db is just empty with no data. 
        context.Database.Migrate();

        return context;
    }
}