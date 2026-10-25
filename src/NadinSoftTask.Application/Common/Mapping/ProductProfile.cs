using AutoMapper;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Products;

namespace NadinSoftTask.Application.Common.Mapping;

public sealed class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(
                destination => destination.ManufacturePhone,
                options => options.MapFrom(source => source.ManufacturePhone.Value))
            .ForMember(
                destination => destination.ManufactureEmail,
                options => options.MapFrom(source => source.ManufactureEmail.Value));
    }
}
