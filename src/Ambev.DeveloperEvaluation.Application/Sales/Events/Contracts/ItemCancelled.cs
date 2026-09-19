namespace Ambev.DeveloperEvaluation.Application.Sales.Events.Contracts;

/// <summary>Integration event: one item of a sale was cancelled.</summary>
public record ItemCancelled(Guid SaleId, long SaleNumber, Guid ItemId, Guid ProductId, decimal NewTotalAmount, DateTime OccurredAt);
