using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
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
/// Contains unit tests for the <see cref="ListSalesHandler"/> class.
/// </summary>
public class ListSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<SaleProfile>()).CreateMapper();
    private readonly ListSalesHandler _handler;

    public ListSalesHandlerTests() => _handler = new ListSalesHandler(_saleRepository, _mapper);

    [Fact(DisplayName = "Given filters and order When listing Then builds the repository query and maps the page")]
    public async Task Handle_Filters_BuildsQueryAndMaps()
    {
        // Given
        IReadOnlyList<Sale> sales = new List<Sale> { SaleTestData.GenerateValidSale(2), SaleTestData.GenerateValidSale(5) };
        SaleListQuery? captured = null;
        _saleRepository.ListAsync(Arg.Do<SaleListQuery>(q => captured = q), Arg.Any<CancellationToken>())
            .Returns((sales, 7));

        var command = new ListSalesCommand
        {
            Page = 2,
            Size = 2,
            Order = "totalAmount desc, saleNumber",
            CustomerName = "Mar*",
            Status = SaleStatus.Active,
            MinTotalAmount = 10m
        };

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        captured.Should().NotBeNull();
        captured!.Page.Should().Be(2);
        captured.Size.Should().Be(2);
        captured.CustomerName.Should().Be("Mar*");
        captured.Status.Should().Be(SaleStatus.Active);
        captured.MinTotalAmount.Should().Be(10m);
        captured.OrderBy.Should().HaveCount(2);
        captured.OrderBy[0].Should().Be(new SaleOrdering("totalAmount", true));

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(7);
        result.CurrentPage.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    [Fact(DisplayName = "Given invalid order When validating Then validation fails")]
    public async Task Validate_InvalidOrder_Fails()
    {
        // Given
        var validator = new ListSalesValidator();
        var command = new ListSalesCommand { Order = "password desc" };

        // When
        var result = await validator.ValidateAsync(command);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ListSalesCommand.Order));
    }

    [Theory(DisplayName = "Given page or size out of range When validating Then validation fails")]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task Validate_PageOrSizeOutOfRange_Fails(int page, int size)
    {
        var result = await new ListSalesValidator().ValidateAsync(new ListSalesCommand { Page = page, Size = size });
        result.IsValid.Should().BeFalse();
    }
}
