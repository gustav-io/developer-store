using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Handler for processing ListSalesCommand requests.
/// </summary>
public class ListSalesHandler : IRequestHandler<ListSalesCommand, ListSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<ListSalesResult> Handle(ListSalesCommand command, CancellationToken cancellationToken)
    {
        var query = new SaleListQuery
        {
            Page = command.Page,
            Size = command.Size,
            OrderBy = SaleOrderByParser.Parse(command.Order),
            SaleNumber = command.SaleNumber,
            CustomerName = command.CustomerName,
            BranchName = command.BranchName,
            Status = command.Status,
            MinDate = command.MinDate,
            MaxDate = command.MaxDate,
            MinTotalAmount = command.MinTotalAmount,
            MaxTotalAmount = command.MaxTotalAmount
        };

        var (items, totalCount) = await _saleRepository.ListAsync(query, cancellationToken);

        return new ListSalesResult
        {
            Items = _mapper.Map<List<SaleResult>>(items),
            TotalCount = totalCount,
            CurrentPage = command.Page,
            PageSize = command.Size
        };
    }
}
