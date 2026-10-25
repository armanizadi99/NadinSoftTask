using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Products;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Domain.Products;

public class ProductTests
{
    private const string ValidOwnerId = "user-1";
    private const string OtherOwnerId = "user-2";
    private const string ValidName = "Acme Widget";
    private const string UpdatedName = "Acme Gadget";
    private static readonly Phone ValidPhone = new("09123456789");
    private static readonly Phone UpdatedPhone = new("09120000000");
    private static readonly Email ValidEmail = new("owner@acme.com");
    private static readonly Email UpdatedEmail = new("updated@acme.com");
    private static readonly DateOnly ValidProduceDate = new(2026, 1, 15);
    private static readonly DateOnly UpdatedProduceDate = new(2027, 2, 20);

    [Fact]
    public void Create_WithValidInput_ReturnsSuccessWithValues()
    {
        Result<Product> result = Product.Create(ValidOwnerId, ValidName, ValidPhone, ValidEmail, true, ValidProduceDate);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);

        Product product = result.Value.ShouldBeOfType<Product>();
        product.Id.ShouldNotBe(Guid.Empty);
        product.OwnerId.ShouldBe(ValidOwnerId);
        product.Name.ShouldBe(ValidName);
        product.ManufacturePhone.ShouldBeSameAs(ValidPhone);
        product.ManufactureEmail.ShouldBeSameAs(ValidEmail);
        product.IsAvailable.ShouldBeTrue();
        product.ProduceDate.ShouldBe(ValidProduceDate);
    }

    [Fact]
    public void Create_KeepsIsAvailableFalse()
    {
        Result<Product> result = Product.Create(ValidOwnerId, ValidName, ValidPhone, ValidEmail, false, ValidProduceDate);

        result.Value!.IsAvailable.ShouldBeFalse();
    }

    [Fact]
    public void Create_GeneratesNewIdEachTime()
    {
        Product first = Product.Create(ValidOwnerId, ValidName, ValidPhone, ValidEmail, true, ValidProduceDate).Value!;
        Product second = Product.Create(ValidOwnerId, ValidName, ValidPhone, ValidEmail, true, ValidProduceDate).Value!;

        first.Id.ShouldNotBe(second.Id);
    }

    [Theory]
    [InlineData(ProductConstraints.MinNameLength)]
    [InlineData(ProductConstraints.MaxNameLength)]
    public void Create_WithNameLengthAtBoundaries_Succeeds(int length)
    {
        string name = new('a', length);

        Result<Product> result = Product.Create(ValidOwnerId, name, ValidPhone, ValidEmail, true, ValidProduceDate);

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Name.ShouldBe(name);
    }

    [Theory]
    [InlineData(ProductConstraints.MinNameLength - 1)]
    [InlineData(ProductConstraints.MaxNameLength + 1)]
    public void Create_WithNameLengthOutOfRange_Throws(int length)
    {
        string name = new('a', length);

        ArgumentException exception = Should.Throw<ArgumentException>(
            () => _ = Product.Create(ValidOwnerId, name, ValidPhone, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceName_Throws(string name)
    {
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => _ = Product.Create(ValidOwnerId, name, ValidPhone, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void Create_WithNullName_Throws()
    {
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => _ = Product.Create(ValidOwnerId, null!, ValidPhone, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void Create_WithNullPhone_Throws()
    {
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => _ = Product.Create(ValidOwnerId, ValidName, null!, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("manufacturePhone");
    }

    [Fact]
    public void Create_WithNullEmail_Throws()
    {
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => _ = Product.Create(ValidOwnerId, ValidName, ValidPhone, null!, true, ValidProduceDate));

        exception.ParamName.ShouldBe("manufactureEmail");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceOwnerId_Throws(string ownerId)
    {
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => _ = Product.Create(ownerId, ValidName, ValidPhone, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("ownerId");
    }

    [Fact]
    public void Create_WithNullOwnerId_Throws()
    {
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => _ = Product.Create(null!, ValidName, ValidPhone, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("ownerId");
    }

    [Fact]
    public void EnsureOwnedBy_WithOwnerId_ReturnsTrue()
    {
        Product product = CreateProduct();

        product.EnsureOwnedBy(ValidOwnerId).ShouldBeTrue();
    }

    [Fact]
    public void EnsureOwnedBy_WithDifferentUserId_ReturnsFalse()
    {
        Product product = CreateProduct();

        product.EnsureOwnedBy(OtherOwnerId).ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureOwnedBy_WithNullOrWhitespace_ReturnsFalse(string? requesterId)
    {
        Product product = CreateProduct();

        product.EnsureOwnedBy(requesterId!).ShouldBeFalse();
    }

    [Fact]
    public void Update_ByOwnerWithValidInput_ChangesAllEditableFields()
    {
        Product product = CreateProduct();

        Result result = product.Update(
            ValidOwnerId,
            UpdatedName,
            UpdatedPhone,
            UpdatedEmail,
            false,
            UpdatedProduceDate);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        product.Name.ShouldBe(UpdatedName);
        product.ManufacturePhone.ShouldBeSameAs(UpdatedPhone);
        product.ManufactureEmail.ShouldBeSameAs(UpdatedEmail);
        product.IsAvailable.ShouldBeFalse();
        product.ProduceDate.ShouldBe(UpdatedProduceDate);
    }

    [Fact]
    public void Update_PreservesIdAndOwnerId()
    {
        Product product = CreateProduct();
        Guid id = product.Id;

        product.Update(ValidOwnerId, UpdatedName, UpdatedPhone, UpdatedEmail, false, UpdatedProduceDate);

        product.Id.ShouldBe(id);
        product.OwnerId.ShouldBe(ValidOwnerId);
    }

    [Fact]
    public void Update_ByNonOwner_ReturnsNotAuthorized()
    {
        Product product = CreateProduct();

        Result result = product.Update(
            OtherOwnerId,
            UpdatedName,
            UpdatedPhone,
            UpdatedEmail,
            false,
            UpdatedProduceDate);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);
    }

    [Fact]
    public void Update_ByNonOwner_DoesNotChangeProduct()
    {
        Product product = CreateProduct();

        product.Update(OtherOwnerId, UpdatedName, UpdatedPhone, UpdatedEmail, false, UpdatedProduceDate);

        product.Name.ShouldBe(ValidName);
        product.ManufacturePhone.ShouldBeSameAs(ValidPhone);
        product.ManufactureEmail.ShouldBeSameAs(ValidEmail);
        product.IsAvailable.ShouldBeTrue();
        product.ProduceDate.ShouldBe(ValidProduceDate);
    }

    [Fact]
    public void Update_ByNonOwnerWithInvalidInput_ReturnsNotAuthorized()
    {
        Product product = CreateProduct();

        Result result = product.Update(OtherOwnerId, "", null!, null!, false, UpdatedProduceDate);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyOrWhitespaceName_Throws(string name)
    {
        Product product = CreateProduct();

        ArgumentException exception = Should.Throw<ArgumentException>(
            () => product.Update(ValidOwnerId, name, ValidPhone, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void Update_WithNullName_Throws()
    {
        Product product = CreateProduct();

        ArgumentException exception = Should.Throw<ArgumentException>(
            () => product.Update(ValidOwnerId, null!, ValidPhone, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("name");
    }

    [Theory]
    [InlineData(ProductConstraints.MinNameLength - 1)]
    [InlineData(ProductConstraints.MaxNameLength + 1)]
    public void Update_WithNameLengthOutOfRange_Throws(int length)
    {
        Product product = CreateProduct();
        string name = new('a', length);

        ArgumentException exception = Should.Throw<ArgumentException>(
            () => product.Update(ValidOwnerId, name, ValidPhone, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void Update_WithNullPhone_Throws()
    {
        Product product = CreateProduct();

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => product.Update(ValidOwnerId, ValidName, null!, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("manufacturePhone");
    }

    [Fact]
    public void Update_WithNullEmail_Throws()
    {
        Product product = CreateProduct();

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => product.Update(ValidOwnerId, ValidName, ValidPhone, null!, true, ValidProduceDate));

        exception.ParamName.ShouldBe("manufactureEmail");
    }

    private static Product CreateProduct() =>
        Product.Create(ValidOwnerId, ValidName, ValidPhone, ValidEmail, true, ValidProduceDate).Value!;
}
