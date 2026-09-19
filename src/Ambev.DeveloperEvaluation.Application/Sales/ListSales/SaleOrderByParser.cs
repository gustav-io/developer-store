using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Parses the <c>_order</c> query parameter ("field [asc|desc], field ...") into whitelisted orderings.
/// Field names are the camelCase names used in the JSON response, as required by .doc/general-api.md.
/// </summary>
public static class SaleOrderByParser
{
    private static readonly string[] AllowedFields =
    {
        "saleNumber", "saleDate", "customerName", "branchName", "status", "totalAmount"
    };

    public static IReadOnlyList<SaleOrdering> Parse(string? order)
    {
        if (!TryParse(order, out var orderings, out var error))
            throw new ArgumentException(error, nameof(order));

        return orderings;
    }

    public static bool TryParse(string? order, out IReadOnlyList<SaleOrdering> orderings, out string? error)
    {
        var result = new List<SaleOrdering>();
        orderings = result;
        error = null;

        if (string.IsNullOrWhiteSpace(order))
            return true;

        var clauses = order.Trim().Trim('"').Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var clause in clauses)
        {
            var parts = clause.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var field = AllowedFields.FirstOrDefault(f => f.Equals(parts[0], StringComparison.OrdinalIgnoreCase));

            if (field == null)
            {
                error = $"Cannot order by '{parts[0]}'. Allowed fields: {string.Join(", ", AllowedFields)}";
                orderings = Array.Empty<SaleOrdering>();
                return false;
            }

            var descending = false;
            if (parts.Length == 2)
            {
                if (parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase))
                    descending = true;
                else if (!parts[1].Equals("asc", StringComparison.OrdinalIgnoreCase))
                {
                    error = $"Invalid direction '{parts[1]}' for '{field}'. Use asc or desc";
                    orderings = Array.Empty<SaleOrdering>();
                    return false;
                }
            }
            else if (parts.Length > 2)
            {
                error = $"Invalid ordering clause '{clause}'";
                orderings = Array.Empty<SaleOrdering>();
                return false;
            }

            result.Add(new SaleOrdering(field, descending));
        }

        return true;
    }
}
