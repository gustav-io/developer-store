using Ambev.DeveloperEvaluation.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;

/// <summary>
/// Query parameters for listing sales. Naming follows .doc/general-api.md.
/// </summary>
public class ListSalesRequest
{
    [FromQuery(Name = "_page")] public int Page { get; set; } = 1;
    [FromQuery(Name = "_size")] public int Size { get; set; } = 10;

    /// <summary>e.g. "saleDate desc, totalAmount".</summary>
    [FromQuery(Name = "_order")] public string? Order { get; set; }

    [FromQuery(Name = "saleNumber")] public long? SaleNumber { get; set; }

    /// <summary>Use '*' as prefix/suffix for partial matches.</summary>
    [FromQuery(Name = "customerName")] public string? CustomerName { get; set; }

    [FromQuery(Name = "branchName")] public string? BranchName { get; set; }
    [FromQuery(Name = "status")] public SaleStatus? Status { get; set; }
    [FromQuery(Name = "_minDate")] public DateTime? MinDate { get; set; }
    [FromQuery(Name = "_maxDate")] public DateTime? MaxDate { get; set; }
    [FromQuery(Name = "_minTotalAmount")] public decimal? MinTotalAmount { get; set; }
    [FromQuery(Name = "_maxTotalAmount")] public decimal? MaxTotalAmount { get; set; }
}
