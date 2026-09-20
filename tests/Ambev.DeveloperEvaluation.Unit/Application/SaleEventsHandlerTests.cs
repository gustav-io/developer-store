using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Events.Contracts;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for <see cref="SaleEventsHandler"/>: domain events become integration messages on the bus.
/// </summary>
public class SaleEventsHandlerTests
{
    private readonly IBus _bus = Substitute.For<IBus>();
    private readonly SaleEventsHandler _handler;

    public SaleEventsHandlerTests()
    {
        _handler = new SaleEventsHandler(_bus, Substitute.For<ILogger<SaleEventsHandler>>());
    }

    [Fact(DisplayName = "Given SaleCreatedEvent When handled Then publishes SaleCreated contract")]
    public async Task Handle_SaleCreated_PublishesContract()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);

        // When
        await _handler.Handle(new SaleCreatedEvent(sale), CancellationToken.None);

        // Then
        await _bus.Received(1).Publish(
            Arg.Is<object>(m => m is SaleCreated && ((SaleCreated)m).SaleId == sale.Id && ((SaleCreated)m).TotalAmount == sale.TotalAmount),
            Arg.Any<IDictionary<string, string>>());
    }

    [Fact(DisplayName = "Given SaleCancelledEvent When handled Then publishes SaleCancelled contract")]
    public async Task Handle_SaleCancelled_PublishesContract()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        sale.Cancel();

        // When
        await _handler.Handle(new SaleCancelledEvent(sale), CancellationToken.None);

        // Then
        await _bus.Received(1).Publish(
            Arg.Is<object>(m => m is SaleCancelled && ((SaleCancelled)m).SaleId == sale.Id),
            Arg.Any<IDictionary<string, string>>());
    }

    [Fact(DisplayName = "Given ItemCancelledEvent When handled Then publishes ItemCancelled with the item and new total")]
    public async Task Handle_ItemCancelled_PublishesContract()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2, 3);
        var item = sale.Items.First();
        sale.CancelItem(item.Id);

        // When
        await _handler.Handle(new ItemCancelledEvent(sale, item), CancellationToken.None);

        // Then
        await _bus.Received(1).Publish(
            Arg.Is<object>(m => m is ItemCancelled && ((ItemCancelled)m).ItemId == item.Id && ((ItemCancelled)m).NewTotalAmount == sale.TotalAmount),
            Arg.Any<IDictionary<string, string>>());
    }

    [Fact(DisplayName = "Given SaleModifiedEvent When handled Then publishes SaleModified contract")]
    public async Task Handle_SaleModified_PublishesContract()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);

        // When
        await _handler.Handle(new SaleModifiedEvent(sale), CancellationToken.None);

        // Then
        await _bus.Received(1).Publish(
            Arg.Is<object>(m => m is SaleModified && ((SaleModified)m).SaleId == sale.Id),
            Arg.Any<IDictionary<string, string>>());
    }
}
