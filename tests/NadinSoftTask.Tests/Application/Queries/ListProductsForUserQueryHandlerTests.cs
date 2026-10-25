using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Application.Queries.ListProductsForUser;
using NadinSoftTask.Domain.Products;
using NadinSoftTask.Tests.Application.TestData;
using NadinSoftTask.Tests.Application.TestSupport;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Application.Queries;

public class ListProductsForUserQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsProductsForRequestedUserId()
    {
        ListProductsForUserQuery query = new(ProductTestData.OwnerId);

        Product firstProduct = ProductTestData.Create(id: 1, ownerId: ProductTestData.OwnerId);
        Product secondProduct = ProductTestData.Create(
            id: 2,
            ownerId: ProductTestData.OwnerId,
            name: "Second Widget",
            emailAddress: "second@acme.com",
            produceDate: ProductTestData.UpdatedProduceDate);

        IReadOnlyList<Product> products = [firstProduct, secondProduct];
        IReadOnlyList<ProductDto> expectedDtos =
            products.Select(product => ProductDtoFactory.From(product)).ToList();

        ProductTestContext context = new();
        context.GivenListByOwner(query.UserId, products);
        context.GivenListMap(expectedDtos, source => source.SequenceEqual(products));

        IReadOnlyList<ProductDto> result = await CreateHandler(context).Handle(
            query,
            context.CancellationToken);

        result.ShouldBe(expectedDtos);

        context.VerifyListByOwnerOnce(query.UserId);
        context.VerifyListMapUsedOnce();
    }

    [Fact]
    public async Task Handle_DoesNotReturnProductsBelongingToOtherUsers()
    {
        ListProductsForUserQuery query = new(ProductTestData.OwnerId);

        Product ownerProduct = ProductTestData.Create(id: 1, ownerId: ProductTestData.OwnerId);
        Product otherProduct = ProductTestData.Create(
            id: 2,
            ownerId: ProductTestData.OtherOwnerId,
            name: "Other Widget",
            emailAddress: "other@acme.com",
            produceDate: ProductTestData.UpdatedProduceDate);

        Product[] allProducts = [ownerProduct, otherProduct];
        ProductDto expectedDto = ProductDtoFactory.From(ownerProduct);

        ProductTestContext context = new();
        context.GivenListByOwner(
            query.UserId,
            (ownerId, _) => allProducts.Where(product => product.OwnerId == ownerId).ToList());
        context.GivenListMap(
            [expectedDto],
            source => source.Count == 1 && source[0].OwnerId == ProductTestData.OwnerId);

        IReadOnlyList<ProductDto> result = await CreateHandler(context).Handle(
            query,
            context.CancellationToken);

        result.ShouldHaveSingleItem().ShouldBe(expectedDto);
        result.ShouldNotContain(dto => dto.OwnerId == ProductTestData.OtherOwnerId);

        context.VerifyListByOwnerOnce(query.UserId);
        context.VerifyListMapUsedOnce();
    }

    [Fact]
    public async Task Handle_WhenUserHasNoProducts_ReturnsEmptyList()
    {
        ListProductsForUserQuery query = new(ProductTestData.OwnerId);

        IReadOnlyList<Product> products = [];
        IReadOnlyList<ProductDto> expectedDtos = [];

        ProductTestContext context = new();
        context.GivenListByOwner(query.UserId, products);
        context.GivenListMap(expectedDtos, source => source.Count == 0);

        IReadOnlyList<ProductDto> result = await CreateHandler(context).Handle(
            query,
            context.CancellationToken);

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        context.VerifyListByOwnerOnce(query.UserId);
        context.VerifyListMapUsedOnce();
    }

    private static ListProductsForUserQueryHandler CreateHandler(ProductTestContext context) =>
        new(
            context.ProductRepository.Object,
            context.Mapper.Object);
}
