using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Ambev.DeveloperEvaluation.ORM;

public class DefaultContext : DbContext
{
    public const string SaleNumberSequence = "SaleNumbers";

    private readonly IPublisher? _publisher;

    public DbSet<User> Users { get; set; }
    public DbSet<Sale> Sales { get; set; }

    public DefaultContext(DbContextOptions<DefaultContext> options, IPublisher? publisher = null) : base(options)
    {
        _publisher = publisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>(SaleNumberSequence).StartsAt(1000).IncrementsBy(1);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Saves and then publishes the domain events collected by tracked aggregates.
    /// Publishing after the commit means a consumer never sees an event for data that was rolled back;
    /// the trade-off is that a crash between commit and publish loses the event (an outbox would close that gap).
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_publisher == null)
            return result;

        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();

            foreach (var domainEvent in events)
                await _publisher.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
public class DefaultContextFactory : IDesignTimeDbContextFactory<DefaultContext>
{
    public DefaultContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var builder = new DbContextOptionsBuilder<DefaultContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        builder.UseNpgsql(
               connectionString,
               b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")
        );

        return new DefaultContext(builder.Options);
    }
}