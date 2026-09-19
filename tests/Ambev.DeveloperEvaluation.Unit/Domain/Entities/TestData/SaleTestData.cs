using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Generates Sale-related test data with Bogus.
/// </summary>
public static class SaleTestData
{
    private static readonly Faker Faker = new();

    public static CustomerRef GenerateCustomer() =>
        new(Guid.NewGuid(), Faker.Person.FullName);

    public static BranchRef GenerateBranch() =>
        new(Guid.NewGuid(), $"{Faker.Address.City()} Branch");

    public static ProductRef GenerateProduct(decimal? unitPrice = null) =>
        new(Guid.NewGuid(), Faker.Commerce.ProductName(), unitPrice ?? Faker.Random.Decimal(1, 500));

    public static SaleItemInput GenerateItem(int quantity = 1, decimal? unitPrice = null) =>
        new(GenerateProduct(unitPrice), quantity);

    /// <summary>
    /// Generates an active sale with the given item quantities (one product per quantity).
    /// </summary>
    public static Sale GenerateValidSale(params int[] quantities)
    {
        var items = (quantities.Length == 0 ? new[] { 1 } : quantities)
            .Select(q => GenerateItem(q));

        return Sale.Create(DateTime.UtcNow, GenerateCustomer(), GenerateBranch(), items);
    }
}
