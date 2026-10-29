using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Products;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Domain.Common;

public class ResultTests
{
    [Fact]
    public void Success_IsSuccessWithNoneError()
    {
        Result result = Result.Success;

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeSameAs(DomainError.None);
    }

    [Fact]
    public void Result_FromDomainError_IsFailure()
    {
        DomainError error = new("Boom", DomainErrorKind.NotAuthorized);

        Result result = error;

        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeSameAs(error);
    }

    [Fact]
    public void Result_FromNullDomainError_Throws()
    {
        DomainError? error = null;

        Should.Throw<ArgumentNullException>(() => _ = (Result)error!);
    }

    [Fact]
    public void Result_FailureWithValueEqualNone_IsFailure()
    {
        DomainError noneLike = new(string.Empty, DomainErrorKind.None);

        Result result = Result.Failure(noneLike);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeSameAs(noneLike);
    }

    [Fact]
    public void GenericResult_FromValue_IsSuccessAndKeepsValue()
    {
        Email email = new("owner@acme.com");

        Result<Email> result = email;

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBeSameAs(email);
        result.Error.ShouldBeSameAs(DomainError.None);
    }

    [Fact]
    public void GenericResult_FromDomainError_IsFailureWithDefaultValue()
    {
        DomainError error = new("Boom", DomainErrorKind.NotAuthorized);

        Result<Email> result = error;

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.ShouldBeSameAs(error);
    }

    [Fact]
    public void GenericResult_FromNullDomainError_Throws()
    {
        DomainError? error = null;

        Should.Throw<ArgumentNullException>(() => _ = (Result<Email>)error!);
    }

    [Fact]
    public void GenericResult_FailureWithValueEqualNone_IsFailure()
    {
        DomainError noneLike = new(string.Empty, DomainErrorKind.None);

        Result<Email> result = Result<Email>.Failure(noneLike);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeSameAs(noneLike);
    }

    [Fact]
    public void GenericSuccess_WithValue_ReturnsSuccessAndKeepsValue()
    {
        Email email = new("owner@acme.com");

        Result<Email> result = Result<Email>.Success(email);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(email);
        result.Error.ShouldBeSameAs(DomainError.None);
    }

    [Fact]
    public void Failure_WithDomainError_ReturnsFailure()
    {
        DomainError error = new("Boom", DomainErrorKind.Validation);

        Result result = Result.Failure(error);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeSameAs(error);
    }

    [Fact]
    public void Failure_WithNullDomainError_Throws()
    {
        Should.Throw<ArgumentNullException>(() => Result.Failure(null!));
    }

    [Fact]
    public void GenericFailure_WithDomainError_ReturnsFailureWithDefaultValue()
    {
        DomainError error = new("Boom", DomainErrorKind.Validation);

        Result<Email> result = Result<Email>.Failure(error);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.ShouldBeSameAs(error);
    }

    [Fact]
    public void GenericFailure_WithNullDomainError_Throws()
    {
        Should.Throw<ArgumentNullException>(() => Result<Email>.Failure(null!));
    }
}
