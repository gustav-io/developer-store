using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Integration.Fixtures;
using Ambev.DeveloperEvaluation.Integration.TestData;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

/// <summary>
/// Exercises SaleRepository against a real PostgreSQL instance (Testcontainers).
/// </summary>
[Collection(PostgresCollection.Name)]
public class SaleRepositoryTests
{
    private readonly PostgresFixture _fixture;

    public SaleRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Create then get should round-trip the aggregate with owned value objects")]
    public async Task CreateAndGet_RoundTrips()
    {
        // Arrange
        var sale = SaleBuilder.Build(unitPrice: 10m, quantities: new[] { 2, 10 });

        // Act
        await using (var context = _fixture.CreateContext())
        {
            await new SaleRepository(context).CreateAsync(sale);
        }

        Sale? loaded;
        await using (var context = _fixture.CreateContext())
        {
            loaded = await new SaleRepository(context).GetByIdAsync(sale.Id);
        }

        // Assert
        loaded.Should().NotBeNull();
        loaded!.SaleNumber.Should().BeGreaterThanOrEqualTo(1000);
        loaded.Customer.Should().Be(sale.Customer);
        loaded.Branch.Should().Be(sale.Branch);
        loaded.Items.Should().HaveCount(2);
        loaded.TotalAmount.Should().Be(20m + 80m);
        loaded.Items.Select(i => i.Product).Should().BeEquivalentTo(sale.Items.Select(i => i.Product));
    }

    [Fact(DisplayName = "Sale numbers should be assigned by the database sequence in increasing order")]
    public async Task Create_AssignsIncreasingSaleNumbers()
    {
        await using var context = _fixture.CreateContext();
        var repository = new SaleRepository(context);

        var first = await repository.CreateAsync(SaleBuilder.Build());
        var second = await repository.CreateAsync(SaleBuilder.Build());

        second.SaleNumber.Should().BeGreaterThan(first.SaleNumber);
    }

    [Fact(DisplayName = "Update after cancelling an item should persist the new total and item flag")]
    public async Task Update_PersistsItemCancellation()
    {
        // Arrange
        var sale = SaleBuilder.Build(unitPrice: 10m, quantities: new[] { 1, 4 });
        await using (var context = _fixture.CreateContext())
            await new SaleRepository(context).CreateAsync(sale);

        // Act
        await using (var context = _fixture.CreateContext())
        {
            var repository = new SaleRepository(context);
            var tracked = (await repository.GetByIdAsync(sale.Id))!;
            tracked.CancelItem(tracked.Items.First(i => i.Quantity == 4).Id);
            await repository.UpdateAsync(tracked);
        }

        // Assert
        await using (var context = _fixture.CreateContext())
        {
            var reloaded = (await new SaleRepository(context).GetByIdAsync(sale.Id))!;
            reloaded.TotalAmount.Should().Be(10m);
            reloaded.Items.Should().ContainSingle(i => i.IsCancelled);
        }
    }

    [Fact(DisplayName = "Update replacing items should insert the new lines and drop the old ones")]
    public async Task Update_ReplacesItems()
    {
        // Arrange
        var sale = SaleBuilder.Build(unitPrice: 10m, quantities: new[] { 1, 2 });
        await using (var context = _fixture.CreateContext())
            await new SaleRepository(context).CreateAsync(sale);

        // Act
        await using (var context = _fixture.CreateContext())
        {
            var repository = new SaleRepository(context);
            var tracked = (await repository.GetByIdAsync(sale.Id))!;
            tracked.Update(tracked.SaleDate, tracked.Customer, tracked.Branch,
                new[] { new SaleItemInput(new Domain.ValueObjects.ProductRef(Guid.NewGuid(), "Replacement", 10m), 10) });
            await repository.UpdateAsync(tracked);
        }

        // Assert
        await using (var context = _fixture.CreateContext())
        {
            var reloaded = (await new SaleRepository(context).GetByIdAsync(sale.Id))!;
            reloaded.Items.Should().ContainSingle(i => i.Product.Name == "Replacement");
            reloaded.TotalAmount.Should().Be(80m);
            context.Set<SaleItem>().Count(i => i.SaleId == sale.Id).Should().Be(1);
        }
    }

    [Fact(DisplayName = "Delete should remove the sale and cascade to its items")]
    public async Task Delete_CascadesItems()
    {
        var sale = SaleBuilder.Build(quantities: new[] { 1, 2 });
        await using (var context = _fixture.CreateContext())
            await new SaleRepository(context).CreateAsync(sale);

        await using (var context = _fixture.CreateContext())
            (await new SaleRepository(context).DeleteAsync(sale.Id)).Should().BeTrue();

        await using (var context = _fixture.CreateContext())
        {
            (await new SaleRepository(context).GetByIdAsync(sale.Id)).Should().BeNull();
            context.Set<SaleItem>().Count(i => i.SaleId == sale.Id).Should().Be(0);
        }
    }

    [Fact(DisplayName = "List should filter by partial customer name, order and page")]
    public async Task List_FiltersOrdersAndPages()
    {
        // Arrange
        var marker = $"Zed{Guid.NewGuid():N}";
        await using (var context = _fixture.CreateContext())
        {
            var repository = new SaleRepository(context);
            await repository.CreateAsync(SaleBuilder.Build($"{marker} Alpha", 10m, 1));   // 10
            await repository.CreateAsync(SaleBuilder.Build($"{marker} Beta", 10m, 5));    // 45
            await repository.CreateAsync(SaleBuilder.Build($"{marker} Gamma", 10m, 10));  // 80
            await repository.CreateAsync(SaleBuilder.Build("Unrelated", 10m, 20));
        }

        // Act
        await using var ctx = _fixture.CreateContext();
        var (items, total) = await new SaleRepository(ctx).ListAsync(new SaleListQuery
        {
            Page = 1,
            Size = 2,
            CustomerName = $"{marker}*",
            OrderBy = new[] { new SaleOrdering("totalAmount", true) }
        });

        // Assert
        total.Should().Be(3);
        items.Should().HaveCount(2);
        items[0].TotalAmount.Should().Be(80m);
        items[1].TotalAmount.Should().Be(45m);
    }

    [Fact(DisplayName = "List should filter by status and amount range")]
    public async Task List_FiltersByStatusAndAmount()
    {
        var marker = $"Q{Guid.NewGuid():N}";
        await using (var context = _fixture.CreateContext())
        {
            var repository = new SaleRepository(context);
            var cancelled = await repository.CreateAsync(SaleBuilder.Build(marker, 10m, 2));
            await repository.CreateAsync(SaleBuilder.Build(marker, 10m, 3));
            cancelled.Cancel();
            await repository.UpdateAsync(cancelled);
        }

        await using var ctx = _fixture.CreateContext();
        var (items, total) = await new SaleRepository(ctx).ListAsync(new SaleListQuery
        {
            CustomerName = marker,
            Status = SaleStatus.Cancelled,
            MinTotalAmount = 15m,
            MaxTotalAmount = 25m
        });

        total.Should().Be(1);
        items.Single().Status.Should().Be(SaleStatus.Cancelled);
    }

    [Fact(DisplayName = "SaveChanges should publish domain events after committing")]
    public async Task SaveChanges_PublishesDomainEvents()
    {
        // Arrange
        var publisher = Substitute.For<IPublisher>();
        await using var context = _fixture.CreateContext(publisher);
        var sale = SaleBuilder.Build();

        // Act
        await new SaleRepository(context).CreateAsync(sale);

        // Assert
        await publisher.Received(1).Publish(Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
        sale.DomainEvents.Should().BeEmpty();
    }
}
