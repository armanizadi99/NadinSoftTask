using NadinSoftTask.Domain.Products;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Domain.Products;

public class EmailTests
{
    [Theory]
    [InlineData("owner@acme.com")]
    [InlineData("a.b+tag@sub.example.co.uk")]
    public void Constructor_WithValidEmail_SetsValue(string value)
    {
        Email email = new(value);

        email.Value.ShouldBe(value);
        email.ToString().ShouldBe(value);
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@exa mple.com")]
    [InlineData("user name@example.com")]
    [InlineData("Bob <bob@example.com>")]
    [InlineData("\"Bob\" <bob@example.com>")]
    [InlineData("user@example.com ")]
    [InlineData(" user@example.com")]
    public void Constructor_WithInvalidEmail_Throws(string value)
    {
        ArgumentException exception = Should.Throw<ArgumentException>(() => new Email(value));

        exception.ParamName.ShouldBe("value");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespace_Throws(string value)
    {
        ArgumentException exception = Should.Throw<ArgumentException>(() => new Email(value));

        exception.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Constructor_WithNull_Throws()
    {
        Should.Throw<ArgumentException>(() => new Email(null!));
    }

    [Theory]
    [InlineData("owner@acme.com")]
    [InlineData("a.b+tag@sub.example.co.uk")]
    public void IsValid_WithValidEmail_ReturnsTrue(string value)
    {
        Email.IsValid(value).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("plainaddress")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@exa mple.com")]
    [InlineData("Bob <bob@example.com>")]
    [InlineData("user@example.com ")]
    [InlineData(" user@example.com")]
    public void IsValid_WithInvalidEmail_ReturnsFalse(string? value)
    {
        Email.IsValid(value).ShouldBeFalse();
    }
}
