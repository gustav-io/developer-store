using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

/// <summary>
/// Maps the Sale aggregate to its read model.
/// </summary>
public class SaleProfile : Profile
{
    public SaleProfile()
    {
        CreateMap<Sale, SaleResult>();
        CreateMap<SaleItem, SaleItemResult>()
            .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.Product.UnitPrice));
    }
}
