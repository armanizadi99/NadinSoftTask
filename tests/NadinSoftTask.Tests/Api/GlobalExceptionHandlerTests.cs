using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NadinSoftTask.Api.Errors;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Api;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenUnhandledException_ReturnsInternalServerErrorProblemDetails()
    {
        CapturingProblemDetailsService problemDetailsService = new();
        GlobalExceptionHandler handler = CreateHandler(problemDetailsService, isDevelopment: false);
        DefaultHttpContext httpContext = CreateHttpContext();
        InvalidOperationException exception = new("Sensitive failure details.");

        bool handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        handled.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);

        ProblemDetailsContext context = problemDetailsService.Context.ShouldNotBeNull();
        context.Exception.ShouldBeSameAs(exception);

        ProblemDetails problemDetails = context.ProblemDetails.ShouldNotBeNull();
        problemDetails.Type.ShouldBe("about:blank");
        problemDetails.Status.ShouldBe(StatusCodes.Status500InternalServerError);
        problemDetails.Title.ShouldBe("Internal Server Error");
        problemDetails.Detail.ShouldBe("An unexpected error occurred while processing the request.");
        problemDetails.Instance.ShouldBe("/api/products");
        problemDetails.Extensions["errorKind"].ShouldBe("InternalServerError");
        problemDetails.Extensions["traceId"].ShouldNotBeNull();
    }

    [Fact]
    public async Task TryHandleAsync_InDevelopment_IncludesExceptionMessage()
    {
        CapturingProblemDetailsService problemDetailsService = new();
        GlobalExceptionHandler handler = CreateHandler(problemDetailsService, isDevelopment: true);
        DefaultHttpContext httpContext = CreateHttpContext();
        InvalidOperationException exception = new("Database connection failed.");

        bool handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        handled.ShouldBeTrue();
        ProblemDetails problemDetails = problemDetailsService.Context.ShouldNotBeNull().ProblemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldBe(exception.Message);
    }

    [Fact]
    public async Task TryHandleAsync_WhenRequestWasAborted_DoesNotHandleException()
    {
        CapturingProblemDetailsService problemDetailsService = new();
        GlobalExceptionHandler handler = CreateHandler(problemDetailsService, isDevelopment: false);
        using CancellationTokenSource cancellationTokenSource = new();
        DefaultHttpContext httpContext = CreateHttpContext();
        httpContext.RequestAborted = cancellationTokenSource.Token;
        cancellationTokenSource.Cancel();

        bool handled = await handler.TryHandleAsync(
            httpContext,
            new OperationCanceledException(cancellationTokenSource.Token),
            CancellationToken.None);

        handled.ShouldBeFalse();
        problemDetailsService.Context.ShouldBeNull();
    }

    [Fact]
    public async Task TryHandleAsync_WhenBadHttpRequestException_ReturnsItsStatusCode()
    {
        CapturingProblemDetailsService problemDetailsService = new();
        GlobalExceptionHandler handler = CreateHandler(problemDetailsService, isDevelopment: false);
        DefaultHttpContext httpContext = CreateHttpContext();
        BadHttpRequestException exception = new(
            "The request body could not be read.",
            StatusCodes.Status400BadRequest);

        bool handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        handled.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);

        ProblemDetails problemDetails = problemDetailsService.Context.ShouldNotBeNull().ProblemDetails.ShouldNotBeNull();
        problemDetails.Type.ShouldBe("about:blank");
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Title.ShouldBe("Bad Request");
        problemDetails.Detail.ShouldBe(exception.Message);
        problemDetails.Extensions["errorKind"].ShouldBe("BadRequest");
    }

    private static GlobalExceptionHandler CreateHandler(
        IProblemDetailsService problemDetailsService,
        bool isDevelopment)
    {
        Mock<IHostEnvironment> environment = new();
        environment
            .SetupGet(item => item.EnvironmentName)
            .Returns(isDevelopment ? Environments.Development : Environments.Production);

        return new GlobalExceptionHandler(
            problemDetailsService,
            environment.Object,
            NullLogger<GlobalExceptionHandler>.Instance);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = "/api/products";

        return httpContext;
    }

    private sealed class CapturingProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetailsContext? Context { get; private set; }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Context = context;
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Context = context;
            return ValueTask.FromResult(true);
        }
    }
}
