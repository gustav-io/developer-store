using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// Maps Sales API requests to commands and results to responses.
/// </summary>
public class SalesProfile : Profile
{
    public SalesProfile()
    {
        CreateMap<SaleItemRequest, SaleItemCommand>();
        CreateMap<CreateSaleRequest, CreateSaleCommand>();
        CreateMap<UpdateSaleRequest, UpdateSaleCommand>();
        CreateMap<SaleResult, SaleResponse>();
        CreateMap<SaleItemResult, SaleItemResponse>();
    }
}
