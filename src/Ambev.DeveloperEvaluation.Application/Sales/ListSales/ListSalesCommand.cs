using Ambev.DeveloperEvaluation.Domain.Enums;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Command for listing sales with paging, ordering and filters (see .doc/general-api.md).
/// </summary>
public class ListSalesCommand : IRequest<ListSalesResult>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;

    /// <summary>"field [asc|desc], field ..." using camelCase response field names.</summary>
    public string? Order { get; set; }

    public long? SaleNumber { get; set; }

    /// <summary>Exact match, or partial with a leading/trailing '*'.</summary>
    public string? CustomerName { get; set; }

    /// <summary>Exact match, or partial with a leading/trailing '*'.</summary>
    public string? BranchName { get; set; }

    public SaleStatus? Status { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
    public decimal? MinTotalAmount { get; set; }
    public decimal? MaxTotalAmount { get; set; }
}
