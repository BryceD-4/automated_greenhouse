/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
    - This file connects and talks to Docker and creates a running PostGreSQL database
*/

using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace Greenhouse.Tests.SharedFiles;

public class PostgresTestContainer
{
    private readonly PostgreSqlContainer _container;
    //Command dynamically creates a connection string for the temporary DB
    //Includes: host, port, username etc...
    public string ConnectionString => _container.GetConnectionString();

    public PostgresTestContainer()
    {
        // Create login for the database using anything we want. This is independent of actual DB. 
        _container = new PostgreSqlBuilder("postgres:16.0")
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    //Start the container (called by PostgresFixture)
    public async Task StartAsync()
    {
        await _container.StartAsync();
    }

    //Stop the container (called by PostgresFixture)
    public async Task StopAsync()
    {
        await _container.StopAsync();
    }
}