using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CancelSaleHandler"/> class.
/// </summary>
public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<SaleProfile>()).CreateMapper();
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests() => _handler = new CancelSaleHandler(_saleRepository, _mapper);

    [Fact(DisplayName = "Given active sale When cancelling Then status becomes Cancelled and is persisted")]
    public async Task Handle_ActiveSale_Cancels()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var result = await _handler.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        // Then
        result.Status.Should().Be(SaleStatus.Cancelled);
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given unknown sale When cancelling Then throws KeyNotFoundException")]
    public async Task Handle_UnknownSale_Throws()
    {
        // Given
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(new CancelSaleCommand(Guid.NewGuid()), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
