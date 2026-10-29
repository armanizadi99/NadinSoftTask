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

        context.GivenProduct(product);
        context.GivenKeyExists(
            command.ProduceDate!.Value,
            command.ManufactureEmail,
            exists: false,
            excludeProductId: ProductId);

        Result<ProductDto> result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        ProductDto dto = result.Value.ShouldNotBeNull();
        dto.Id.ShouldBe(ProductId);
        dto.OwnerId.ShouldBe(ProductTestData.OwnerId);
        dto.Name.ShouldBe(command.Name);
        dto.ManufacturePhone.ShouldBe(command.ManufacturePhone);
        dto.ManufactureEmail.ShouldBe(command.ManufactureEmail);
        dto.IsAvailable.ShouldBe(command.IsAvailable!.Value);
        dto.ProduceDate.ShouldBe(command.ProduceDate!.Value);

        product.Name.ShouldBe(command.Name);
        product.ManufacturePhone.Value.ShouldBe(command.ManufacturePhone);
        product.ManufactureEmail.Value.ShouldBe(command.ManufactureEmail);
        product.IsAvailable.ShouldBe(command.IsAvailable!.Value);
        product.ProduceDate.ShouldBe(command.ProduceDate!.Value);

        context.VerifyGetByIdOnce(ProductId);
        context.VerifyKeyExistsOnce(command.ProduceDate!.Value, command.ManufactureEmail, ProductId);
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

        context.GivenProduct(product);
        context.GivenKeyExists(
            ProductTestData.ProduceDate,
            ProductTestData.EmailAddress,
            exists: false,
            excludeProductId: ProductId);

        Result<ProductDto> result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        ProductDto dto = result.Value.ShouldNotBeNull();
        dto.Id.ShouldBe(ProductId);
        dto.Name.ShouldBe(command.Name);
        dto.ManufacturePhone.ShouldBe(command.ManufacturePhone);
        dto.ProduceDate.ShouldBe(command.ProduceDate!.Value);

        context.VerifyKeyExistsOnce(ProductTestData.ProduceDate, ProductTestData.EmailAddress, ProductId);
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
    }

    [Fact]
    public async Task Handle_WhenCurrentUserDoesNotOwnProduct_ReturnsNotAuthorized()
    {
        EditProductCommand command = CreateCommand();
        Product product = ProductTestData.Create(id: ProductId, ownerId: ProductTestData.OtherOwnerId);
        ProductTestContext context = new();

        context.GivenProduct(product);

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
        context.VerifyKeyExistsNever();
        context.VerifyNotSaved();
    }

    [Fact]
    public async Task Handle_WhenCurrentUserDoesNotOwnProductAndKeyConflicts_ReturnsNotAuthorizedBeforeDuplicateCheck()
    {
        EditProductCommand command = CreateCommand();
        Product product = ProductTestData.Create(id: ProductId, ownerId: ProductTestData.OtherOwnerId);
        ProductTestContext context = new();

        context.GivenProduct(product);

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
        context.VerifyKeyExistsNever();
        context.VerifyNotSaved();
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
            context.Mapper);
}
