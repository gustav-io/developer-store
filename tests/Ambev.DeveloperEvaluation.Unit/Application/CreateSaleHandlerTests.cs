using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CreateSaleHandler"/> class.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<SaleProfile>()).CreateMapper();
        _handler = new CreateSaleHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid command When creating sale Then persists aggregate and returns result")]
    public async Task Handle_ValidCommand_PersistsAndReturnsResult()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.CustomerId.Should().Be(command.CustomerId);
        result.CustomerName.Should().Be(command.CustomerName);
        result.BranchId.Should().Be(command.BranchId);
        result.Items.Should().HaveCount(command.Items.Count);
        result.TotalAmount.Should().Be(result.Items.Sum(i => i.TotalAmount));
        await _saleRepository.Received(1).CreateAsync(
            Arg.Is<Sale>(s => s.Items.Count == command.Items.Count),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given item above 20 units When creating sale Then throws DomainException and does not persist")]
    public async Task Handle_QuantityAbove20_ThrowsAndDoesNotPersist()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.Items = new List<SaleItemCommand> { CreateSaleHandlerTestData.GenerateItem(21) };

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>();
        await _saleRepository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given command without sale date When creating sale Then defaults to now")]
    public async Task Handle_NoSaleDate_DefaultsToUtcNow()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.SaleDate = null;
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.SaleDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
