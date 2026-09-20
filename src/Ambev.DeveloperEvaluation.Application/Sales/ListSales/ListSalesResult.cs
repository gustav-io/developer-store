using Ambev.DeveloperEvaluation.Application.Sales.Common;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// One page of sales plus paging metadata.
/// </summary>
public class ListSalesResult
{
    public List<SaleResult> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}
