using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository interface for the Sale aggregate.
/// </summary>
public interface ISaleRepository
{
    /// <summary>Persists a new sale (with its items).</summary>
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>Loads a sale with its items, or null.</summary>
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Persists changes made to a tracked sale.</summary>
    Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>Hard-deletes a sale. Returns false when it does not exist.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lists sales with paging, ordering and filters; also returns the unpaged count.</summary>
    Task<(IReadOnlyList<Sale> Items, int TotalCount)> ListAsync(SaleListQuery query, CancellationToken cancellationToken = default);
}
