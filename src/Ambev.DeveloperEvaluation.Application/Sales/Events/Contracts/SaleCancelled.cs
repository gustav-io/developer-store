namespace Ambev.DeveloperEvaluation.Application.Sales.Events.Contracts;

/// <summary>Integration event: a sale was cancelled.</summary>
public record SaleCancelled(Guid SaleId, long SaleNumber, DateTime OccurredAt);
