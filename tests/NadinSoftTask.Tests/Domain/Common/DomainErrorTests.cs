using NadinSoftTask.Domain.Common;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Domain.Common;

public class DomainErrorTests
{
    [Fact]
    public void None_HasEmptyMessageAndNoneKind()
    {
        DomainError.None.Message.ShouldBeEmpty();
        DomainError.None.Kind.ShouldBe(DomainErrorKind.None);
    }

    [Fact]
    public void Constructor_AssignsMessageAndKind()
    {
        DomainError error = new("Not allowed", DomainErrorKind.NotAuthorized);

        error.Message.ShouldBe("Not allowed");
        error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);
    }

    [Fact]
    public void Constructor_DoesNotValidateMessage()
    {
        DomainError error = new(string.Empty, DomainErrorKind.NotAuthorized);

        error.Message.ShouldBeEmpty();
    }

    [Fact]
    public void Records_WithSameValues_AreEqual()
    {
        DomainError first = new("Not allowed", DomainErrorKind.NotAuthorized);
        DomainError second = new("Not allowed", DomainErrorKind.NotAuthorized);

        first.ShouldBe(second);
    }
}
