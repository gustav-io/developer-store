using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// One line of a sale as received from the API.
/// </summary>
public class SaleItemCommand
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Command for creating a sale.
/// </summary>
public class CreateSaleCommand : IRequest<SaleResult>
{
    /// <summary>When the sale was made (UTC). Defaults to now.</summary>
    public DateTime? SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<SaleItemCommand> Items { get; set; } = new();
}
