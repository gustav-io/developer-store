using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSaleHandler"/> class.
/// </summary>
public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<SaleProfile>()).CreateMapper();
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests() => _handler = new UpdateSaleHandler(_saleRepository, _mapper);

    [Fact(DisplayName = "Given existing sale When updating Then replaces items and persists")]
    public async Task Handle_ExistingSale_ReplacesItems()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        var command = new UpdateSaleCommand
        {
            Id = sale.Id,
            SaleDate = sale.SaleDate,
            CustomerId = Guid.NewGuid(),
            CustomerName = "New Customer",
            BranchId = sale.Branch.Id,
            BranchName = sale.Branch.Name,
            Items = new List<SaleItemCommand> { CreateSaleHandlerTestData.GenerateItem(10) }
        };

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.CustomerName.Should().Be("New Customer");
        result.Items.Should().ContainSingle(i => i.Quantity == 10 && i.DiscountPercent == 0.20m);
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given cancelled sale When updating Then throws DomainException")]
    public async Task Handle_CancelledSale_Throws()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        sale.Cancel();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        var command = new UpdateSaleCommand
        {
            Id = sale.Id,
            SaleDate = sale.SaleDate,
            CustomerId = sale.Customer.Id,
            CustomerName = sale.Customer.Name,
            BranchId = sale.Branch.Id,
            BranchName = sale.Branch.Name,
            Items = new List<SaleItemCommand> { CreateSaleHandlerTestData.GenerateItem(1) }
        };

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>();
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }
}
