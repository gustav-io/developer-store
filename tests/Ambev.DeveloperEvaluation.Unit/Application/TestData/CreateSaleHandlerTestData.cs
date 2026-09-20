using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

/// <summary>
/// Generates CreateSaleCommand instances with Bogus.
/// </summary>
public static class CreateSaleHandlerTestData
{
    private static readonly Faker<SaleItemCommand> ItemFaker = new Faker<SaleItemCommand>()
        .RuleFor(i => i.ProductId, _ => Guid.NewGuid())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.UnitPrice, f => f.Random.Decimal(1, 200))
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 20));

    private static readonly Faker<CreateSaleCommand> CommandFaker = new Faker<CreateSaleCommand>()
        .RuleFor(c => c.SaleDate, f => f.Date.Recent(5).ToUniversalTime())
        .RuleFor(c => c.CustomerId, _ => Guid.NewGuid())
        .RuleFor(c => c.CustomerName, f => f.Person.FullName)
        .RuleFor(c => c.BranchId, _ => Guid.NewGuid())
        .RuleFor(c => c.BranchName, f => $"{f.Address.City()} Branch")
        .RuleFor(c => c.Items, f => ItemFaker.Generate(f.Random.Int(1, 3)));

    public static CreateSaleCommand GenerateValidCommand() => CommandFaker.Generate();

    public static SaleItemCommand GenerateItem(int quantity)
    {
        var item = ItemFaker.Generate();
        item.Quantity = quantity;
        return item;
    }
}
