using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Application.Queries.ListProductsForUser;
using NadinSoftTask.Domain.Common;
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

        ProductTestContext context = new();
        context.GivenUser(query.UserId);
        context.GivenListByOwner(query.UserId, products);

        Result<IReadOnlyList<ProductDto>> result = await CreateHandler(context).Handle(
            query,
            context.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        IReadOnlyList<ProductDto> dtos = result.Value.ShouldNotBeNull();
        dtos.Count.ShouldBe(2);
        dtos[0].Id.ShouldBe(firstProduct.Id);
        dtos[0].OwnerId.ShouldBe(firstProduct.OwnerId);
        dtos[1].Id.ShouldBe(secondProduct.Id);
        dtos[1].Name.ShouldBe(secondProduct.Name);

        context.VerifyUserLookupOnce(query.UserId);
        context.VerifyListByOwnerOnce(query.UserId);
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

        ProductTestContext context = new();
        context.GivenUser(query.UserId);
        context.GivenListByOwner(
            query.UserId,
            (ownerId, _) => allProducts.Where(product => product.OwnerId == ownerId).ToList());

        Result<IReadOnlyList<ProductDto>> result = await CreateHandler(context).Handle(
            query,
            context.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        ProductDto dto = result.Value.ShouldNotBeNull().ShouldHaveSingleItem();
        dto.Id.ShouldBe(ownerProduct.Id);
        dto.OwnerId.ShouldBe(ProductTestData.OwnerId);
        result.Value.ShouldNotContain(candidate => candidate.OwnerId == ProductTestData.OtherOwnerId);

        context.VerifyUserLookupOnce(query.UserId);
        context.VerifyListByOwnerOnce(query.UserId);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoProducts_ReturnsEmptyList()
    {
        ListProductsForUserQuery query = new(ProductTestData.OwnerId);

        IReadOnlyList<Product> products = [];

        ProductTestContext context = new();
        context.GivenUser(query.UserId);
        context.GivenListByOwner(query.UserId, products);

        Result<IReadOnlyList<ProductDto>> result = await CreateHandler(context).Handle(
            query,
            context.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeEmpty();

        context.VerifyUserLookupOnce(query.UserId);
        context.VerifyListByOwnerOnce(query.UserId);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFound()
    {
        ListProductsForUserQuery query = new("missing-user");

        ProductTestContext context = new();
        context.GivenMissingUser(query.UserId);

        Result<IReadOnlyList<ProductDto>> result = await CreateHandler(context).Handle(
            query,
            context.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotFound);

        context.VerifyUserLookupOnce(query.UserId);
        context.VerifyListByOwnerNever();
    }

    private static ListProductsForUserQueryHandler CreateHandler(ProductTestContext context) =>
        new(
            context.UserRepository.Object,
            context.ProductRepository.Object,
            context.Mapper);
}
