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
        ProductDto expectedDto = ProductDtoFactory.From(command, ProductTestData.OwnerId, createdProductId);

        context.GivenKeyExists(command.ProduceDate, command.ManufactureEmail, exists: false);
        context.GivenAddAssignsId(createdProductId, product =>
            product.Id == 0 &&
            product.OwnerId == ProductTestData.OwnerId &&
            product.Name == command.Name &&
            product.ManufacturePhone.Value == command.ManufacturePhone &&
            product.ManufactureEmail.Value == command.ManufactureEmail &&
            product.IsAvailable == command.IsAvailable &&
            product.ProduceDate == command.ProduceDate);
        context.GivenMap(expectedDto, product => product.Id == createdProductId);

        Result<ProductDto> result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        result.Value.ShouldBe(expectedDto);

        context.VerifyKeyExistsOnce(command.ProduceDate, command.ManufactureEmail);
        context.VerifyAddedOnce();
        context.VerifyNotDeleted();
        context.VerifyProductMapUsedOnce();
        context.VerifySavedOnce();
    }

    [Fact]
    public async Task Handle_WhenCompositeKeyAlreadyExists_ReturnsConflictAndDoesNotPersist()
    {
        CreateProductCommand command = CreateCommand();
        ProductTestContext context = new();

        context.GivenKeyExists(command.ProduceDate, command.ManufactureEmail, exists: true);

        Result<ProductDto> result = await CreateHandler(context).Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.Conflict);

        context.VerifyKeyExistsOnce(command.ProduceDate, command.ManufactureEmail);
        context.VerifyNotAdded();
        context.VerifyNotDeleted();
        context.VerifyProductMapNotUsed();
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
            context.Mapper.Object);
}
