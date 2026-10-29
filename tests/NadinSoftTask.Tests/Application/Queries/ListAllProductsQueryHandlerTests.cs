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

        ProductTestContext context = new();
        context.GivenListAll(products);

        Result<IReadOnlyList<ProductDto>> result = await CreateHandler(context).Handle(
            new ListAllProductsQuery(),
            context.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        IReadOnlyList<ProductDto> dtos = result.Value.ShouldNotBeNull();
        dtos.Count.ShouldBe(2);
        dtos[0].Id.ShouldBe(firstProduct.Id);
        dtos[0].OwnerId.ShouldBe(firstProduct.OwnerId);
        dtos[0].Name.ShouldBe(firstProduct.Name);
        dtos[1].Id.ShouldBe(secondProduct.Id);
        dtos[1].ManufactureEmail.ShouldBe(secondProduct.ManufactureEmail.Value);

        context.VerifyListAllOnce();
    }

    [Fact]
    public async Task Handle_WhenNoProductsExist_ReturnsEmptyList()
    {
        IReadOnlyList<Product> products = [];

        ProductTestContext context = new();
        context.GivenListAll(products);

        Result<IReadOnlyList<ProductDto>> result = await CreateHandler(context).Handle(
            new ListAllProductsQuery(),
            context.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeEmpty();

        context.VerifyListAllOnce();
    }

    private static ListAllProductsQueryHandler CreateHandler(ProductTestContext context) =>
        new(
            context.ProductRepository.Object,
            context.Mapper);
}
