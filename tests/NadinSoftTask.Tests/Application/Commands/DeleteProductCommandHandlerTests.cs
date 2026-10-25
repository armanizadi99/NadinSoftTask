using NadinSoftTask.Application.Commands.DeleteProduct;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Products;
using NadinSoftTask.Tests.Application.TestData;
using NadinSoftTask.Tests.Application.TestSupport;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Application.Commands;

public class DeleteProductCommandHandlerTests
{
    private const int ProductId = 42;

    [Fact]
    public async Task Handle_WhenOwnerDeletesExistingProduct_DeletesAndSavesChanges()
    {
        DeleteProductCommand command = new(ProductId);
        Product product = ProductTestData.Create(id: ProductId, ownerId: ProductTestData.OwnerId);
        ProductTestContext context = new();

        context.GivenProduct(product);
        context.GivenDelete(product);

        Result result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);

        context.VerifyGetByIdOnce(ProductId);
        context.VerifyDeletedOnce();
        context.VerifySavedOnce();
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ReturnsNotFound()
    {
        DeleteProductCommand command = new(ProductId);
        ProductTestContext context = new();

        context.GivenMissingProduct(ProductId);

        Result result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotFound);

        context.VerifyGetByIdOnce(ProductId);
        context.VerifyNotDeleted();
        context.VerifyNotSaved();
    }

    [Fact]
    public async Task Handle_WhenCurrentUserDoesNotOwnProduct_ReturnsNotAuthorized()
    {
        DeleteProductCommand command = new(ProductId);
        Product product = ProductTestData.Create(id: ProductId, ownerId: ProductTestData.OtherOwnerId);
        ProductTestContext context = new();

        context.GivenProduct(product);

        Result result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);

        context.VerifyGetByIdOnce(ProductId);
        context.VerifyNotDeleted();
        context.VerifyNotSaved();
    }

    private static DeleteProductCommandHandler CreateHandler(ProductTestContext context) =>
        new(
            context.ProductRepository.Object,
            context.UnitOfWork.Object,
            context.CurrentUser.Object);
}
