using Ambev.DeveloperEvaluation.Application.Sales.Events.Contracts;
using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Bridges Sale domain events to the message bus: logs the event and publishes a flat integration contract.
/// Domain events carry entities and may change freely; contracts are the public schema other services depend on.
/// </summary>
public class SaleEventsHandler :
    INotificationHandler<SaleCreatedEvent>,
    INotificationHandler<SaleModifiedEvent>,
    INotificationHandler<SaleCancelledEvent>,
    INotificationHandler<ItemCancelledEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<SaleEventsHandler> _logger;

    public SaleEventsHandler(IBus bus, ILogger<SaleEventsHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        var sale = notification.Sale;
        _logger.LogInformation("SaleCreated {SaleId} number {SaleNumber} total {TotalAmount}", sale.Id, sale.SaleNumber, sale.TotalAmount);

        return _bus.Publish(new SaleCreated(sale.Id, sale.SaleNumber, sale.Customer.Id, sale.Branch.Id, sale.TotalAmount, DateTime.UtcNow));
    }

    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        var sale = notification.Sale;
        _logger.LogInformation("SaleModified {SaleId} number {SaleNumber} total {TotalAmount}", sale.Id, sale.SaleNumber, sale.TotalAmount);

        return _bus.Publish(new SaleModified(sale.Id, sale.SaleNumber, sale.TotalAmount, DateTime.UtcNow));
    }

    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        var sale = notification.Sale;
        _logger.LogInformation("SaleCancelled {SaleId} number {SaleNumber}", sale.Id, sale.SaleNumber);

        return _bus.Publish(new SaleCancelled(sale.Id, sale.SaleNumber, DateTime.UtcNow));
    }

    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        var (sale, item) = notification;
        _logger.LogInformation("ItemCancelled {SaleId} item {ItemId} product {ProductId} new total {TotalAmount}", sale.Id, item.Id, item.Product.Id, sale.TotalAmount);

        return _bus.Publish(new ItemCancelled(sale.Id, sale.SaleNumber, item.Id, item.Product.Id, sale.TotalAmount, DateTime.UtcNow));
    }
}
