using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of ISaleRepository using Entity Framework Core.
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <summary>
    /// The sale is already tracked (loaded through GetByIdAsync in the same scope), so the change tracker
    /// detects modified fields, added items and orphaned items removed by the aggregate.
    /// </summary>
    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (sale == null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(IReadOnlyList<Sale> Items, int TotalCount)> ListAsync(SaleListQuery query, CancellationToken cancellationToken = default)
    {
        var sales = _context.Sales.AsNoTracking().Include(s => s.Items).AsQueryable();

        if (query.SaleNumber.HasValue)
            sales = sales.Where(s => s.SaleNumber == query.SaleNumber.Value);
        if (!string.IsNullOrWhiteSpace(query.CustomerName))
            sales = sales.Where(s => EF.Functions.ILike(s.Customer.Name, ToLikePattern(query.CustomerName)));
        if (!string.IsNullOrWhiteSpace(query.BranchName))
            sales = sales.Where(s => EF.Functions.ILike(s.Branch.Name, ToLikePattern(query.BranchName)));
        if (query.Status.HasValue)
            sales = sales.Where(s => s.Status == query.Status.Value);
        if (query.MinDate.HasValue)
            sales = sales.Where(s => s.SaleDate >= query.MinDate.Value);
        if (query.MaxDate.HasValue)
            sales = sales.Where(s => s.SaleDate <= query.MaxDate.Value);
        if (query.MinTotalAmount.HasValue)
            sales = sales.Where(s => s.TotalAmount >= query.MinTotalAmount.Value);
        if (query.MaxTotalAmount.HasValue)
            sales = sales.Where(s => s.TotalAmount <= query.MaxTotalAmount.Value);

        var totalCount = await sales.CountAsync(cancellationToken);

        var items = await ApplyOrdering(sales, query.OrderBy)
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// '*' at either end of the value means partial match (see .doc/general-api.md); otherwise exact, case-insensitive.
    /// </summary>
    private static string ToLikePattern(string value) => value.Replace('*', '%');

    private static IQueryable<Sale> ApplyOrdering(IQueryable<Sale> sales, IReadOnlyList<SaleOrdering> orderBy)
    {
        if (orderBy.Count == 0)
            return sales.OrderBy(s => s.SaleNumber);

        IOrderedQueryable<Sale>? ordered = null;
        foreach (var (field, descending) in orderBy)
        {
            ordered = field switch
            {
                "saleNumber" => Apply(s => s.SaleNumber),
                "saleDate" => Apply(s => s.SaleDate),
                "customerName" => Apply(s => s.Customer.Name),
                "branchName" => Apply(s => s.Branch.Name),
                "status" => Apply(s => s.Status),
                "totalAmount" => Apply(s => s.TotalAmount),
                _ => throw new ArgumentOutOfRangeException(nameof(orderBy), $"Unknown ordering field '{field}'")
            };

            IOrderedQueryable<Sale> Apply<TKey>(Expression<Func<Sale, TKey>> key) =>
                (ordered, descending) switch
                {
                    (null, false) => sales.OrderBy(key),
                    (null, true) => sales.OrderByDescending(key),
                    (_, false) => ordered.ThenBy(key),
                    (_, true) => ordered.ThenByDescending(key)
                };
        }

        return ordered!;
    }
}
