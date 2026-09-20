using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="DeleteSaleHandler"/> class.
/// </summary>
public class DeleteSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly DeleteSaleHandler _handler;

    public DeleteSaleHandlerTests() => _handler = new DeleteSaleHandler(_saleRepository);

    [Fact(DisplayName = "Given existing sale When deleting Then returns success")]
    public async Task Handle_ExistingSale_ReturnsSuccess()
    {
        // Given
        var id = Guid.NewGuid();
        _saleRepository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        // When
        var result = await _handler.Handle(new DeleteSaleCommand(id), CancellationToken.None);

        // Then
        result.Success.Should().BeTrue();
    }

    [Fact(DisplayName = "Given unknown sale When deleting Then throws KeyNotFoundException")]
    public async Task Handle_UnknownSale_Throws()
    {
        // Given
        _saleRepository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        // When
        var act = () => _handler.Handle(new DeleteSaleCommand(Guid.NewGuid()), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
