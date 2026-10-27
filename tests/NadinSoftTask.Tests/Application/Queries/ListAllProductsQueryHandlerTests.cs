using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Application.Queries.ListAllProducts;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Products;
using NadinSoftTask.Tests.Application.TestData;
using NadinSoftTask.Tests.Application.TestSupport;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Application.Queries;

public class ListAllProductsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenProductsExist_ReturnsAllMappedProducts()
    {
        Product firstProduct = ProductTestData.Create(id: 1, ownerId: ProductTestData.OwnerId);
        Product secondProduct = ProductTestData.Create(
            id: 2,
            ownerId: ProductTestData.OtherOwnerId,
            name: "Second Widget",
            phoneNumber: "09121111111",
            emailAddress: "second@acme.com",
            isAvailable: false,
            produceDate: ProductTestData.UpdatedProduceDate);

        IReadOnlyList<Product> products = [firstProduct, secondProduct];
        IReadOnlyList<ProductDto> expectedDtos =
            products.Select(product => ProductDtoFactory.From(product)).ToList();

        ProductTestContext context = new();
        context.GivenListAll(products);
        context.GivenListMap(expectedDtos, source => source.SequenceEqual(products));

        Result<IReadOnlyList<ProductDto>> result = await CreateHandler(context).Handle(
            new ListAllProductsQuery(),
            context.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        result.Value.ShouldBe(expectedDtos);

        context.VerifyListAllOnce();
        context.VerifyListMapUsedOnce();
    }

    [Fact]
    public async Task Handle_WhenNoProductsExist_ReturnsEmptyList()
    {
        IReadOnlyList<Product> products = [];
        IReadOnlyList<ProductDto> expectedDtos = [];

        ProductTestContext context = new();
        context.GivenListAll(products);
        context.GivenListMap(expectedDtos, source => source.Count == 0);

        Result<IReadOnlyList<ProductDto>> result = await CreateHandler(context).Handle(
            new ListAllProductsQuery(),
            context.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeEmpty();

        context.VerifyListAllOnce();
        context.VerifyListMapUsedOnce();
    }

    private static ListAllProductsQueryHandler CreateHandler(ProductTestContext context) =>
        new(
            context.ProductRepository.Object,
            context.Mapper.Object);
}
