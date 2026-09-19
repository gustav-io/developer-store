using Ambev.DeveloperEvaluation.ORM;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Fixtures;

/// <summary>
/// Starts one PostgreSQL container per test collection and applies the EF Core migrations to it.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public DefaultContext CreateContext(IPublisher? publisher = null)
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new DefaultContext(options, publisher);
    }
}
