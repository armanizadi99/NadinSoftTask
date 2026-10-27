using NadinSoftTask.Application.Commands.EditProduct;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Products;
using NadinSoftTask.Tests.Application.TestData;
using NadinSoftTask.Tests.Application.TestSupport;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Application.Commands;

public class EditProductCommandHandlerTests
{
    private const int ProductId = 42;

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesProductMapsItAndSavesChanges()
    {
        EditProductCommand command = CreateCommand();
        Product product = ProductTestData.Create(id: ProductId, ownerId: ProductTestData.OwnerId);
        ProductTestContext context = new();
        ProductDto expectedDto = ProductDtoFactory.From(command, ProductTestData.OwnerId);

        context.GivenProduct(product);
        context.GivenKeyExists(
            command.ProduceDate!.Value,
            command.ManufactureEmail,
            exists: false,
            excludeProductId: ProductId);
        context.GivenMap(expectedDto, updated =>
            updated.Id == ProductId &&
            updated.Name == command.Name &&
            updated.ManufacturePhone.Value == command.ManufacturePhone &&
            updated.ManufactureEmail.Value == command.ManufactureEmail &&
            updated.IsAvailable == command.IsAvailable!.Value &&
            updated.ProduceDate == command.ProduceDate!.Value);

        Result<ProductDto> result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        result.Value.ShouldBe(expectedDto);

        product.Name.ShouldBe(command.Name);
        product.ManufacturePhone.Value.ShouldBe(command.ManufacturePhone);
        product.ManufactureEmail.Value.ShouldBe(command.ManufactureEmail);
        product.IsAvailable.ShouldBe(command.IsAvailable!.Value);
        product.ProduceDate.ShouldBe(command.ProduceDate!.Value);

        context.VerifyGetByIdOnce(ProductId);
        context.VerifyKeyExistsOnce(command.ProduceDate!.Value, command.ManufactureEmail, ProductId);
        context.VerifyProductMapUsedOnce();
        context.VerifySavedOnce();
    }

    [Fact]
    public async Task Handle_WhenCompositeKeyIsUnchanged_ExcludesCurrentProductAndSucceeds()
    {
        EditProductCommand command = new(
            ProductId,
            ProductTestData.UpdatedProductName,
            ProductTestData.UpdatedPhoneNumber,
            ProductTestData.EmailAddress,
            false,
            ProductTestData.ProduceDate);

        Product product = ProductTestData.Create(id: ProductId, ownerId: ProductTestData.OwnerId);
        ProductTestContext context = new();
        ProductDto expectedDto = ProductDtoFactory.From(command, ProductTestData.OwnerId);

        context.GivenProduct(product);
        context.GivenKeyExists(
            ProductTestData.ProduceDate,
            ProductTestData.EmailAddress,
            exists: false,
            excludeProductId: ProductId);
        context.GivenMap(expectedDto, updated => updated.Id == ProductId);

        Result<ProductDto> result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedDto);

        context.VerifyKeyExistsOnce(ProductTestData.ProduceDate, ProductTestData.EmailAddress, ProductId);
        context.VerifyProductMapUsedOnce();
        context.VerifySavedOnce();
    }

    [Fact]
    public async Task Handle_WhenUpdatedCompositeKeyConflicts_ReturnsConflictAndLeavesProductUnchanged()
    {
        EditProductCommand command = CreateCommand();
        Product product = ProductTestData.Create(id: ProductId, ownerId: ProductTestData.OwnerId);
        ProductTestContext context = new();

        context.GivenProduct(product);
        context.GivenKeyExists(
            command.ProduceDate!.Value,
            command.ManufactureEmail,
            exists: true,
            excludeProductId: ProductId);

        Result<ProductDto> result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.Conflict);

        product.Name.ShouldBe(ProductTestData.ProductName);
        product.ManufacturePhone.Value.ShouldBe(ProductTestData.PhoneNumber);
        product.ManufactureEmail.Value.ShouldBe(ProductTestData.EmailAddress);
        product.IsAvailable.ShouldBeTrue();
        product.ProduceDate.ShouldBe(ProductTestData.ProduceDate);

        context.VerifyKeyExistsOnce(command.ProduceDate!.Value, command.ManufactureEmail, ProductId);
        context.VerifyNotSaved();
        context.VerifyProductMapNotUsed();
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ReturnsNotFound()
    {
        EditProductCommand command = CreateCommand();
        ProductTestContext context = new();

        context.GivenMissingProduct(ProductId);

        Result<ProductDto> result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotFound);

        context.VerifyGetByIdOnce(ProductId);
        context.VerifyKeyExistsNever();
        context.VerifyNotSaved();
        context.VerifyProductMapNotUsed();
    }

    [Fact]
    public async Task Handle_WhenCurrentUserDoesNotOwnProduct_ReturnsNotAuthorized()
    {
        EditProductCommand command = CreateCommand();
        Product product = ProductTestData.Create(id: ProductId, ownerId: ProductTestData.OtherOwnerId);
        ProductTestContext context = new();

        context.GivenProduct(product);
        context.GivenKeyExists(
            command.ProduceDate!.Value,
            command.ManufactureEmail,
            exists: false,
            excludeProductId: ProductId);

        Result<ProductDto> result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);

        product.Name.ShouldBe(ProductTestData.ProductName);
        product.ManufacturePhone.Value.ShouldBe(ProductTestData.PhoneNumber);
        product.ManufactureEmail.Value.ShouldBe(ProductTestData.EmailAddress);
        product.IsAvailable.ShouldBeTrue();
        product.ProduceDate.ShouldBe(ProductTestData.ProduceDate);

        context.VerifyGetByIdOnce(ProductId);
        context.VerifyNotSaved();
        context.VerifyProductMapNotUsed();
    }

    private static EditProductCommand CreateCommand() =>
        new(
            ProductId,
            ProductTestData.UpdatedProductName,
            ProductTestData.UpdatedPhoneNumber,
            ProductTestData.UpdatedEmailAddress,
            false,
            ProductTestData.UpdatedProduceDate);

    private static EditProductCommandHandler CreateHandler(ProductTestContext context) =>
        new(
            context.ProductRepository.Object,
            context.UnitOfWork.Object,
            context.CurrentUser.Object,
            context.Mapper.Object);
}
