namespace Ambev.DeveloperEvaluation.Application.Sales.Events.Contracts;

/// <summary>Integration event: a sale's header or items changed.</summary>
public record SaleModified(Guid SaleId, long SaleNumber, decimal TotalAmount, DateTime OccurredAt);
