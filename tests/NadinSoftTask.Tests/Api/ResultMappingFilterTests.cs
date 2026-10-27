using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NadinSoftTask.Api.Filters;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Api;

public class ResultMappingFilterTests
{
    private static readonly ProductDto SampleProduct = new(
        id: 1,
        ownerId: "3f2504e0-4f89-41d3-9a0c-0305e82c3301",
        name: "Wireless Mouse",
        manufacturePhone: "09123456789",
        manufactureEmail: "manufacturer@example.com",
        isAvailable: true,
        produceDate: new DateOnly(2026, 10, 27));

    [Fact]
    public void OnResultExecuting_WhenGenericResultSucceeds_UnwrapsValueAndKeepsStatus()
    {
        using ServiceProvider provider = CreateServiceProvider();
        ResultMappingFilter filter = CreateFilter(provider);
        Result<ProductDto> result = SampleProduct;
        ObjectResult objectResult = new(result) { StatusCode = StatusCodes.Status201Created };
        ResultExecutingContext context = CreateContext(objectResult);

        filter.OnResultExecuting(context);

        context.Result.ShouldBeSameAs(objectResult);
        objectResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
        objectResult.Value.ShouldBe(SampleProduct);
    }

    [Fact]
    public void OnResultExecuting_WhenNonGenericResultSucceedsWithNoContent_ReturnsNoContent()
    {
        using ServiceProvider provider = CreateServiceProvider();
        ResultMappingFilter filter = CreateFilter(provider);
        ObjectResult objectResult = new(Result.Success)
        {
            StatusCode = StatusCodes.Status204NoContent
        };
        ResultExecutingContext context = CreateContext(objectResult);

        filter.OnResultExecuting(context);

        context.Result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public void OnResultExecuting_WhenValidationFails_ReturnsValidationProblemDetails()
    {
        using ServiceProvider provider = CreateServiceProvider();
        ResultMappingFilter filter = CreateFilter(provider);
        DomainError error = new(
            "Product name cannot be empty or whitespace.",
            DomainErrorKind.Validation);
        Result<ProductDto> result = error;
        ResultExecutingContext context = CreateContext(new OkObjectResult(result));

        filter.OnResultExecuting(context);

        ObjectResult objectResult = context.Result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        objectResult.ContentTypes.ShouldContain("application/problem+json");

        ValidationProblemDetails problem = objectResult.Value.ShouldBeOfType<ValidationProblemDetails>();
        problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problem.Detail.ShouldBe(error.Message);
        problem.Errors["general"].ShouldContain(error.Message);
        problem.Extensions["errorKind"].ShouldBe(DomainErrorKind.Validation.ToString());
        problem.Extensions["traceId"].ShouldNotBeNull();
    }

    [Theory]
    [InlineData(DomainErrorKind.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(DomainErrorKind.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(DomainErrorKind.NotAuthorized, StatusCodes.Status403Forbidden)]
    public void OnResultExecuting_WhenNonValidationErrorFails_MapsStatusAndProblemDetails(
        DomainErrorKind kind,
        int expectedStatusCode)
    {
        using ServiceProvider provider = CreateServiceProvider();
        ResultMappingFilter filter = CreateFilter(provider);
        DomainError error = new("Boom", kind);
        Result result = error;
        ResultExecutingContext context = CreateContext(new OkObjectResult(result));

        filter.OnResultExecuting(context);

        ObjectResult objectResult = context.Result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(expectedStatusCode);
        objectResult.ContentTypes.ShouldContain("application/problem+json");

        ProblemDetails problem = objectResult.Value.ShouldBeOfType<ProblemDetails>();
        problem.Status.ShouldBe(expectedStatusCode);
        problem.Detail.ShouldBe(error.Message);
        problem.Extensions["errorKind"].ShouldBe(kind.ToString());
        problem.Extensions["traceId"].ShouldNotBeNull();
    }

    [Fact]
    public void OnResultExecuting_WhenNotAuthorizedAndMarkedAsUnauthorized_Returns401()
    {
        using ServiceProvider provider = CreateServiceProvider();
        ResultMappingFilter filter = CreateFilter(provider);
        Result result = new DomainError("Invalid credentials.", DomainErrorKind.NotAuthorized);
        ResultExecutingContext context = CreateContext(
            new OkObjectResult(result),
            new List<object> { new MapNotAuthorizedAsUnauthorizedAttribute() });

        filter.OnResultExecuting(context);

        ObjectResult objectResult = context.Result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public void OnResultExecuting_WhenValueIsNotAResult_LeavesResultUntouched()
    {
        using ServiceProvider provider = CreateServiceProvider();
        ResultMappingFilter filter = CreateFilter(provider);
        OkObjectResult objectResult = new(new { Value = 42 });
        ResultExecutingContext context = CreateContext(objectResult);

        filter.OnResultExecuting(context);

        context.Result.ShouldBeSameAs(objectResult);
        objectResult.Value.ShouldBe(new { Value = 42 });
    }

    private static ServiceProvider CreateServiceProvider()
    {
        ServiceCollection services = new();
        services.AddControllers();

        return services.BuildServiceProvider();
    }

    private static ResultMappingFilter CreateFilter(ServiceProvider provider) =>
        new(provider.GetRequiredService<ProblemDetailsFactory>());

    private static ResultExecutingContext CreateContext(
        IActionResult result,
        IList<object>? endpointMetadata = null)
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Path = "/api/products";

        ActionContext actionContext = new(
            httpContext,
            new RouteData(),
            new ActionDescriptor
            {
                EndpointMetadata = endpointMetadata ?? new List<object>()
            });

        return new ResultExecutingContext(
            actionContext,
            Array.Empty<IFilterMetadata>(),
            result,
            controller: new object());
    }
}
