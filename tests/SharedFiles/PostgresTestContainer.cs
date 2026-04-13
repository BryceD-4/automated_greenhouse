/*

This file connects and talks to Docker and creates a running PostGreSQL database
*/

using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace Greenhouse.Tests.SharedFiles;

public class PostgresTestContainer
{
    private readonly PostgreSqlContainer _container;

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

    public async Task StartAsync()
    {
        await _container.StartAsync();
    }

    public async Task StopAsync()
    {
        await _container.StopAsync();
    }
}