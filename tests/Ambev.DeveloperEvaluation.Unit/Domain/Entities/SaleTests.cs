using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the Sale aggregate: creation, discounts, cancellation and invariants.
/// </summary>
public class SaleTests
{
    [Fact(DisplayName = "Creating a sale should compute item totals and raise SaleCreatedEvent")]
    public void Given_ValidItems_When_Created_Then_TotalsAndEventAreSet()
    {
        // Arrange
        var customer = SaleTestData.GenerateCustomer();
        var branch = SaleTestData.GenerateBranch();
        var items = new[]
        {
            new SaleItemInput(SaleTestData.GenerateProduct(10m), 2),   // 20.00, no discount
            new SaleItemInput(SaleTestData.GenerateProduct(10m), 5),   // 50.00 - 10% = 45.00
        };

        // Act
        var sale = Sale.Create(DateTime.UtcNow, customer, branch, items);

        // Assert
        sale.Status.Should().Be(SaleStatus.Active);
        sale.Customer.Should().Be(customer);
        sale.Branch.Should().Be(branch);
        sale.Items.Should().HaveCount(2);
        sale.TotalAmount.Should().Be(65m);
        sale.DomainEvents.Should().ContainSingle(e => e is SaleCreatedEvent);
    }

    [Theory(DisplayName = "Item discount should follow the quantity tiers")]
    [InlineData(3, 0, 30)]
    [InlineData(4, 4, 36)]
    [InlineData(10, 20, 80)]
    [InlineData(20, 40, 160)]
    public void Given_Quantity_When_Created_Then_ItemDiscountMatchesTier(int quantity, decimal expectedDiscount, decimal expectedTotal)
    {
        // Arrange
        var item = new SaleItemInput(SaleTestData.GenerateProduct(10m), quantity);

        // Act
        var sale = Sale.Create(DateTime.UtcNow, SaleTestData.GenerateCustomer(), SaleTestData.GenerateBranch(), new[] { item });

        // Assert
        var saleItem = sale.Items.Single();
        saleItem.DiscountAmount.Should().Be(expectedDiscount);
        saleItem.TotalAmount.Should().Be(expectedTotal);
        sale.TotalAmount.Should().Be(expectedTotal);
    }

    [Fact(DisplayName = "Selling more than 20 identical items should be rejected")]
    public void Given_QuantityAbove20_When_Created_Then_ThrowsDomainException()
    {
        // Act
        var act = () => SaleTestData.GenerateValidSale(21);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*20*");
    }

    [Fact(DisplayName = "A sale without items should be rejected")]
    public void Given_NoItems_When_Created_Then_ThrowsDomainException()
    {
        // Act
        var act = () => Sale.Create(DateTime.UtcNow, SaleTestData.GenerateCustomer(), SaleTestData.GenerateBranch(), Array.Empty<SaleItemInput>());

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "The same product twice in one sale should be rejected")]
    public void Given_DuplicateProduct_When_Created_Then_ThrowsDomainException()
    {
        // Arrange
        var product = SaleTestData.GenerateProduct();
        var items = new[] { new SaleItemInput(product, 15), new SaleItemInput(product, 15) };

        // Act
        var act = () => Sale.Create(DateTime.UtcNow, SaleTestData.GenerateCustomer(), SaleTestData.GenerateBranch(), items);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*duplicate*");
    }

    [Fact(DisplayName = "Cancelling a sale should set status and raise SaleCancelledEvent")]
    public void Given_ActiveSale_When_Cancelled_Then_StatusIsCancelled()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale(2);
        sale.ClearDomainEvents();

        // Act
        sale.Cancel();

        // Assert
        sale.Status.Should().Be(SaleStatus.Cancelled);
        sale.UpdatedAt.Should().NotBeNull();
        sale.DomainEvents.Should().ContainSingle(e => e is SaleCancelledEvent);
    }

    [Fact(DisplayName = "Cancelling an item should recalculate the total and raise ItemCancelledEvent")]
    public void Given_ActiveSale_When_ItemCancelled_Then_TotalExcludesItem()
    {
        // Arrange
        var items = new[]
        {
            new SaleItemInput(SaleTestData.GenerateProduct(10m), 2),
            new SaleItemInput(SaleTestData.GenerateProduct(10m), 5),
        };
        var sale = Sale.Create(DateTime.UtcNow, SaleTestData.GenerateCustomer(), SaleTestData.GenerateBranch(), items);
        var toCancel = sale.Items.First(i => i.Quantity == 5);
        sale.ClearDomainEvents();

        // Act
        sale.CancelItem(toCancel.Id);

        // Assert
        toCancel.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(20m);
        sale.DomainEvents.Should().ContainSingle(e => e is ItemCancelledEvent);
    }

    [Fact(DisplayName = "Cancelling an already cancelled item should be rejected")]
    public void Given_CancelledItem_When_CancelledAgain_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale(2, 3);
        var item = sale.Items.First();
        sale.CancelItem(item.Id);

        // Act
        var act = () => sale.CancelItem(item.Id);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Cancelling an unknown item should be rejected")]
    public void Given_UnknownItem_When_Cancelled_Then_ThrowsKeyNotFound()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale(2);

        // Act
        var act = () => sale.CancelItem(Guid.NewGuid());

        // Assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact(DisplayName = "A cancelled sale should reject modifications")]
    public void Given_CancelledSale_When_Updated_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale(2);
        sale.Cancel();

        // Act
        var update = () => sale.Update(DateTime.UtcNow, SaleTestData.GenerateCustomer(), SaleTestData.GenerateBranch(), new[] { SaleTestData.GenerateItem(1) });
        var cancelAgain = () => sale.Cancel();
        var cancelItem = () => sale.CancelItem(sale.Items.First().Id);

        // Assert
        update.Should().Throw<DomainException>();
        cancelAgain.Should().Throw<DomainException>();
        cancelItem.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Updating a sale should replace items, recalculate and raise SaleModifiedEvent")]
    public void Given_ActiveSale_When_Updated_Then_ItemsReplaced()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale(2);
        sale.ClearDomainEvents();
        var newCustomer = SaleTestData.GenerateCustomer();
        var newItems = new[] { new SaleItemInput(SaleTestData.GenerateProduct(10m), 10) }; // 100 - 20%

        // Act
        sale.Update(sale.SaleDate, newCustomer, sale.Branch, newItems);

        // Assert
        sale.Customer.Should().Be(newCustomer);
        sale.Items.Should().ContainSingle();
        sale.TotalAmount.Should().Be(80m);
        sale.UpdatedAt.Should().NotBeNull();
        sale.DomainEvents.Should().ContainSingle(e => e is SaleModifiedEvent);
    }
}
