using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

/// <summary>
/// Converts API item lines into domain inputs.
/// </summary>
public static class SaleItemCommandExtensions
{
    public static IEnumerable<SaleItemInput> ToSaleItemInputs(this IEnumerable<SaleItemCommand> items) =>
        items.Select(i => new SaleItemInput(new ProductRef(i.ProductId, i.ProductName, i.UnitPrice), i.Quantity));
}
