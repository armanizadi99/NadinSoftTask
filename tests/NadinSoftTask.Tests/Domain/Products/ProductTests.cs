using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Products;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Domain.Products;

public class ProductTests
{
    private const string ValidName = "Acme Widget";
    private static readonly Phone ValidPhone = new("09123456789");
    private static readonly Email ValidEmail = new("owner@acme.com");
    private static readonly DateOnly ValidProduceDate = new(2026, 1, 15);

    [Fact]
    public void Create_WithValidInput_ReturnsSuccessWithValues()
    {
        Result<Product> result = Product.Create(ValidName, ValidPhone, ValidEmail, true, ValidProduceDate);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);

        Product product = result.Value.ShouldBeOfType<Product>();
        product.Id.ShouldNotBe(Guid.Empty);
        product.Name.ShouldBe(ValidName);
        product.ManufacturePhone.ShouldBeSameAs(ValidPhone);
        product.ManufactureEmail.ShouldBeSameAs(ValidEmail);
        product.IsAvailable.ShouldBeTrue();
        product.ProduceDate.ShouldBe(ValidProduceDate);
    }

    [Fact]
    public void Create_KeepsIsAvailableFalse()
    {
        Result<Product> result = Product.Create(ValidName, ValidPhone, ValidEmail, false, ValidProduceDate);

        result.Value!.IsAvailable.ShouldBeFalse();
    }

    [Fact]
    public void Create_GeneratesNewIdEachTime()
    {
        Product first = Product.Create(ValidName, ValidPhone, ValidEmail, true, ValidProduceDate).Value!;
        Product second = Product.Create(ValidName, ValidPhone, ValidEmail, true, ValidProduceDate).Value!;

        first.Id.ShouldNotBe(second.Id);
    }

    [Theory]
    [InlineData(ProductConstraints.MinNameLength)]
    [InlineData(ProductConstraints.MaxNameLength)]
    public void Create_WithNameLengthAtBoundaries_Succeeds(int length)
    {
        string name = new('a', length);

        Result<Product> result = Product.Create(name, ValidPhone, ValidEmail, true, ValidProduceDate);

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
            () => _ = Product.Create(name, ValidPhone, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceName_Throws(string name)
    {
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => _ = Product.Create(name, ValidPhone, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void Create_WithNullName_Throws()
    {
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => _ = Product.Create(null!, ValidPhone, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void Create_WithNullPhone_Throws()
    {
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => _ = Product.Create(ValidName, null!, ValidEmail, true, ValidProduceDate));

        exception.ParamName.ShouldBe("manufacturePhone");
    }

    [Fact]
    public void Create_WithNullEmail_Throws()
    {
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => _ = Product.Create(ValidName, ValidPhone, null!, true, ValidProduceDate));

        exception.ParamName.ShouldBe("manufactureEmail");
    }
}
