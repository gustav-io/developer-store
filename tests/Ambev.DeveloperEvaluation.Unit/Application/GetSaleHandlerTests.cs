using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="GetSaleHandler"/> class.
/// </summary>
public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<SaleProfile>()).CreateMapper();
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests() => _handler = new GetSaleHandler(_saleRepository, _mapper);

    [Fact(DisplayName = "Given existing sale When getting Then returns mapped result")]
    public async Task Handle_ExistingSale_ReturnsResult()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2, 5);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var result = await _handler.Handle(new GetSaleCommand(sale.Id), CancellationToken.None);

        // Then
        result.Id.Should().Be(sale.Id);
        result.Items.Should().HaveCount(2);
        result.TotalAmount.Should().Be(sale.TotalAmount);
    }

    [Fact(DisplayName = "Given unknown sale When getting Then throws KeyNotFoundException")]
    public async Task Handle_UnknownSale_ThrowsKeyNotFound()
    {
        // Given
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(new GetSaleCommand(Guid.NewGuid()), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
