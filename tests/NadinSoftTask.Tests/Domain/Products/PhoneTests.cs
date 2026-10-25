using NadinSoftTask.Domain.Products;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Domain.Products;

public class PhoneTests
{
    [Theory]
    [InlineData("1234")]
    [InlineData("09123456789")]
    [InlineData("12345678901234")]
    public void Constructor_WithValidLength_SetsValue(string value)
    {
        Phone phone = new(value);

        phone.Value.ShouldBe(value);
        phone.ToString().ShouldBe(value);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012345")]
    public void Constructor_WithInvalidLength_Throws(string value)
    {
        ArgumentException exception = Should.Throw<ArgumentException>(() => new Phone(value));

        exception.ParamName.ShouldBe("value");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("     ")]
    public void Constructor_WithEmptyOrWhitespace_Throws(string value)
    {
        ArgumentException exception = Should.Throw<ArgumentException>(() => new Phone(value));

        exception.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Constructor_WithNull_Throws()
    {
        Should.Throw<ArgumentException>(() => new Phone(null!));
    }

    [Fact]
    public void Bounds_MatchSpecification()
    {
        ProductConstraints.MinPhoneLength.ShouldBe(4);
        ProductConstraints.MaxPhoneLength.ShouldBe(14);
    }
}
