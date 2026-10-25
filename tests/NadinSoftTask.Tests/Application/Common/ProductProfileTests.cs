using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Application.Common.Mapping;
using NadinSoftTask.Domain.Products;
using NadinSoftTask.Tests.Application.TestData;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Application.Common;

public class ProductProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        MapperConfiguration configuration = new(
            config => config.AddProfile<ProductProfile>(),
            NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_Product_MapsAllFields()
    {
        Product product = ProductTestData.Create(id: 42, ownerId: ProductTestData.OwnerId);
        IMapper mapper = new MapperConfiguration(
            config => config.AddProfile<ProductProfile>(),
            NullLoggerFactory.Instance).CreateMapper();

        ProductDto dto = mapper.Map<ProductDto>(product);

        dto.ShouldBe(ProductDtoFactory.From(product));
    }
}
