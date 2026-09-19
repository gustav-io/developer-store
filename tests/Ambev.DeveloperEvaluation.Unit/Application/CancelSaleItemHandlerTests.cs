using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CancelSaleItemHandler"/> class.
/// </summary>
public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<SaleProfile>()).CreateMapper();
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests() => _handler = new CancelSaleItemHandler(_saleRepository, _mapper);

    [Fact(DisplayName = "Given active sale When cancelling one item Then total is recalculated")]
    public async Task Handle_ActiveSale_CancelsItem()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2, 3);
        var item = sale.Items.First();
        var expectedTotal = sale.TotalAmount - item.TotalAmount;
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var result = await _handler.Handle(new CancelSaleItemCommand(sale.Id, item.Id), CancellationToken.None);

        // Then
        result.Items.Should().ContainSingle(i => i.Id == item.Id && i.IsCancelled);
        result.TotalAmount.Should().Be(expectedTotal);
    }

    [Fact(DisplayName = "Given unknown item When cancelling Then throws KeyNotFoundException")]
    public async Task Handle_UnknownItem_Throws()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var act = () => _handler.Handle(new CancelSaleItemCommand(sale.Id, Guid.NewGuid()), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }
}
