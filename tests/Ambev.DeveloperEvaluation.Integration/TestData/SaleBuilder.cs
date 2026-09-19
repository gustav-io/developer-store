using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Integration.TestData;

/// <summary>
/// Builds Sale aggregates for persistence tests.
/// </summary>
public static class SaleBuilder
{
    private static readonly Faker Faker = new();

    public static Sale Build(string? customerName = null, decimal unitPrice = 10m, params int[] quantities)
    {
        var items = (quantities.Length == 0 ? new[] { 1 } : quantities)
            .Select(q => new SaleItemInput(new ProductRef(Guid.NewGuid(), Faker.Commerce.ProductName(), unitPrice), q));

        return Sale.Create(
            DateTime.UtcNow,
            new CustomerRef(Guid.NewGuid(), customerName ?? Faker.Person.FullName),
            new BranchRef(Guid.NewGuid(), $"{Faker.Address.City()} Branch"),
            items);
    }
}
