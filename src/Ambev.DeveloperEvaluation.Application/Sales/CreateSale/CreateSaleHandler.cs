using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Handler for processing CreateSaleCommand requests.
/// </summary>
public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Builds the Sale aggregate (which enforces business rules) and persists it.
    /// </summary>
    public async Task<SaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = Sale.Create(
            command.SaleDate ?? DateTime.UtcNow,
            new CustomerRef(command.CustomerId, command.CustomerName),
            new BranchRef(command.BranchId, command.BranchName),
            command.Items.ToSaleItemInputs());

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);
        return _mapper.Map<SaleResult>(created);
    }
}
