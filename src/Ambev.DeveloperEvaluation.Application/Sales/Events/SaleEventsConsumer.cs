using Ambev.DeveloperEvaluation.Application.Sales.Events.Contracts;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Sample subscriber for Sales integration events. In production this would live in another service
/// (e.g. billing or reporting); here it demonstrates the message round-trip through the bus.
/// </summary>
public class SaleEventsConsumer :
    IHandleMessages<SaleCreated>,
    IHandleMessages<SaleModified>,
    IHandleMessages<SaleCancelled>,
    IHandleMessages<ItemCancelled>
{
    private readonly ILogger<SaleEventsConsumer> _logger;

    public SaleEventsConsumer(ILogger<SaleEventsConsumer> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCreated message)
    {
        _logger.LogInformation("Consumed SaleCreated for sale {SaleNumber} ({TotalAmount})", message.SaleNumber, message.TotalAmount);
        return Task.CompletedTask;
    }

    public Task Handle(SaleModified message)
    {
        _logger.LogInformation("Consumed SaleModified for sale {SaleNumber} ({TotalAmount})", message.SaleNumber, message.TotalAmount);
        return Task.CompletedTask;
    }

    public Task Handle(SaleCancelled message)
    {
        _logger.LogInformation("Consumed SaleCancelled for sale {SaleNumber}", message.SaleNumber);
        return Task.CompletedTask;
    }

    public Task Handle(ItemCancelled message)
    {
        _logger.LogInformation("Consumed ItemCancelled for sale {SaleNumber}, item {ItemId}", message.SaleNumber, message.ItemId);
        return Task.CompletedTask;
    }
}
