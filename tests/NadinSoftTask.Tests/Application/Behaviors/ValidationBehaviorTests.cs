using FluentValidation;
using MediatR;
using NadinSoftTask.Application.Common.Behaviors;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Application.Queries.ListProductsForUser;
using NadinSoftTask.Domain.Common;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Application.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenNoValidators_CallsNext()
    {
        bool nextCalled = false;
        ValidationBehavior<TestRequest, Result<string>> behavior =
            new(Array.Empty<IValidator<TestRequest>>());

        Result<string> result = await behavior.Handle(
            new TestRequest("valid"),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult((Result<string>)"ok");
            },
            CancellationToken.None);

        nextCalled.ShouldBeTrue();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("ok");
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_CallsNext()
    {
        bool nextCalled = false;
        ValidationBehavior<TestRequest, Result<string>> behavior =
            new(new IValidator<TestRequest>[] { new TestRequestValidator() });

        Result<string> result = await behavior.Handle(
            new TestRequest("valid"),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult((Result<string>)"ok");
            },
            CancellationToken.None);

        nextCalled.ShouldBeTrue();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("ok");
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsValidationFailureAndDoesNotCallNext()
    {
        bool nextCalled = false;
        ValidationBehavior<TestRequest, Result<string>> behavior =
            new(new IValidator<TestRequest>[] { new TestRequestValidator() });

        Result<string> result = await behavior.Handle(
            new TestRequest(string.Empty),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult((Result<string>)"ok");
            },
            CancellationToken.None);

        nextCalled.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.Validation);
        result.Error.Message.ShouldBe("Name is required.");
    }

    [Fact]
    public async Task Handle_WhenMultipleValidatorsFail_AggregatesTheirMessages()
    {
        ValidationBehavior<TestRequest, Result<string>> behavior =
            new(new IValidator<TestRequest>[]
            {
                new TestRequestValidator(),
                new SecondTestRequestValidator()
            });

        Result<string> result = await behavior.Handle(
            new TestRequest(string.Empty),
            _ => Task.FromResult((Result<string>)"ok"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.Validation);
        result.Error.Message.ShouldContain("Name is required.");
        result.Error.Message.ShouldContain("Second name error.");
    }

    [Fact]
    public async Task Handle_WhenResponseIsNonGenericResult_ReturnsValidationFailure()
    {
        ValidationBehavior<TestCommand, Result> behavior =
            new(new IValidator<TestCommand>[] { new TestCommandValidator() });

        Result result = await behavior.Handle(
            new TestCommand(string.Empty),
            _ => Task.FromResult(Result.Success),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.Validation);
        result.Error.Message.ShouldBe("Command name is required.");
    }

    [Fact]
    public async Task Handle_WhenQueryValidationFails_ReturnsValidationFailureForQueryResult()
    {
        ValidationBehavior<ListProductsForUserQuery, Result<IReadOnlyList<ProductDto>>> behavior =
            new(new IValidator<ListProductsForUserQuery>[]
            {
                new ListProductsForUserQueryValidator()
            });

        Result<IReadOnlyList<ProductDto>> result = await behavior.Handle(
            new ListProductsForUserQuery(string.Empty),
            _ => throw new InvalidOperationException("Next should not be called."),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.Validation);
        result.Error.Message.ShouldBe("User id cannot be empty or whitespace.");
    }

    [Fact]
    public async Task Handle_WhenResponseIsNotResult_Throws()
    {
        ValidationBehavior<TestRequest, string> behavior =
            new(new IValidator<TestRequest>[] { new TestRequestValidator() });

        await Should.ThrowAsync<InvalidOperationException>(() =>
            behavior.Handle(
                new TestRequest(string.Empty),
                _ => Task.FromResult("unused"),
                CancellationToken.None));
    }

    private sealed record TestRequest(string Name) : IRequest<Result<string>>;

    private sealed record TestCommand(string Name) : IRequest<Result>;

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .WithMessage("Name is required.");
        }
    }

    private sealed class SecondTestRequestValidator : AbstractValidator<TestRequest>
    {
        public SecondTestRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .WithMessage("Second name error.");
        }
    }

    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(command => command.Name)
                .NotEmpty()
                .WithMessage("Command name is required.");
        }
    }
}
