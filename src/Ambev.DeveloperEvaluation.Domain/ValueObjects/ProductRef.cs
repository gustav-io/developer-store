namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// External identity of a product with the denormalized name and unit price at the time of sale.
/// </summary>
public record ProductRef(Guid Id, string Name, decimal UnitPrice);
