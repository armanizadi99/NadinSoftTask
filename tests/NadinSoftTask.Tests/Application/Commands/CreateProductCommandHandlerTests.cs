using NadinSoftTask.Application.Commands.CreateProduct;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Products;
using NadinSoftTask.Tests.Application.TestData;
using NadinSoftTask.Tests.Application.TestSupport;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Application.Commands;

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_AddsProductMapsItAndSavesChanges()
    {
        const int createdProductId = 42;
        CreateProductCommand command = CreateCommand();
        ProductTestContext context = new();

        context.GivenKeyExists(command.ProduceDate!.Value, command.ManufactureEmail, exists: false);
        context.GivenAddAssignsId(createdProductId, product =>
            product.Id == 0 &&
            product.OwnerId == ProductTestData.OwnerId &&
            product.Name == command.Name &&
            product.ManufacturePhone.Value == command.ManufacturePhone &&
            product.ManufactureEmail.Value == command.ManufactureEmail &&
            product.IsAvailable == command.IsAvailable!.Value &&
            product.ProduceDate == command.ProduceDate!.Value);

        Result<ProductDto> result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        ProductDto dto = result.Value.ShouldNotBeNull();
        dto.Id.ShouldBe(createdProductId);
        dto.OwnerId.ShouldBe(ProductTestData.OwnerId);
        dto.Name.ShouldBe(command.Name);
        dto.ManufacturePhone.ShouldBe(command.ManufacturePhone);
        dto.ManufactureEmail.ShouldBe(command.ManufactureEmail);
        dto.IsAvailable.ShouldBe(command.IsAvailable!.Value);
        dto.ProduceDate.ShouldBe(command.ProduceDate!.Value);

        context.VerifyKeyExistsOnce(command.ProduceDate!.Value, command.ManufactureEmail);
        context.VerifyAddedOnce();
        context.VerifyNotDeleted();
        context.VerifySavedOnce();
    }

    [Fact]
    public async Task Handle_WhenCompositeKeyAlreadyExists_ReturnsConflictAndDoesNotPersist()
    {
        CreateProductCommand command = CreateCommand();
        ProductTestContext context = new();

        context.GivenKeyExists(command.ProduceDate!.Value, command.ManufactureEmail, exists: true);

        Result<ProductDto> result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.Conflict);

        context.VerifyKeyExistsOnce(command.ProduceDate!.Value, command.ManufactureEmail);
        context.VerifyNotAdded();
        context.VerifyNotDeleted();
        context.VerifyNotSaved();
    }

    private static CreateProductCommand CreateCommand() =>
        new(
            ProductTestData.ProductName,
            ProductTestData.PhoneNumber,
            ProductTestData.EmailAddress,
            true,
            ProductTestData.ProduceDate);

    private static CreateProductCommandHandler CreateHandler(ProductTestContext context) =>
        new(
            context.ProductRepository.Object,
            context.UnitOfWork.Object,
            context.CurrentUser.Object,
            context.Mapper);
}
