namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>
/// One product line of a sale.
/// </summary>
public class SaleItemRequest
{
    /// <summary>External product identifier.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Product description denormalized at the time of sale.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Unit price at the time of sale.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Quantity (1 to 20). 4+ units get 10% off, 10+ get 20% off.</summary>
    public int Quantity { get; set; }
}
