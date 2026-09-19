using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// One ordering clause: a whitelisted field name (camelCase, as in the API response) and direction.
/// </summary>
public record SaleOrdering(string Field, bool Descending);

/// <summary>
/// Paging, ordering and filtering options for listing sales.
/// String filters support a leading/trailing '*' wildcard.
/// </summary>
public class SaleListQuery
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public IReadOnlyList<SaleOrdering> OrderBy { get; set; } = Array.Empty<SaleOrdering>();

    public long? SaleNumber { get; set; }
    public string? CustomerName { get; set; }
    public string? BranchName { get; set; }
    public SaleStatus? Status { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
    public decimal? MinTotalAmount { get; set; }
    public decimal? MaxTotalAmount { get; set; }
}
