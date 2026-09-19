namespace Ambev.DeveloperEvaluation.Application.Sales.Events.Contracts;

/// <summary>Integration event: a sale was created. Flat, versionable contract for other services.</summary>
public record SaleCreated(Guid SaleId, long SaleNumber, Guid CustomerId, Guid BranchId, decimal TotalAmount, DateTime OccurredAt);
