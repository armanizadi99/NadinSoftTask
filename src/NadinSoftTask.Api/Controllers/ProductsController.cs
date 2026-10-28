using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NadinSoftTask.Api.Contracts.Products;
using NadinSoftTask.Api.OpenApi;
using NadinSoftTask.Application.Commands.CreateProduct;
using NadinSoftTask.Application.Commands.DeleteProduct;
using NadinSoftTask.Application.Commands.EditProduct;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Application.Queries.ListAllProducts;
using NadinSoftTask.Application.Queries.ListProductsForUser;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Api.Controllers;

/// <summary>
/// Product catalogue endpoints.
/// </summary>
/// <remarks>
/// Creating, updating, and deleting products requires an authenticated user. The two GET
/// endpoints are anonymous. Only the user who created a product can update or delete it.
/// </remarks>
[ApiController]
[Route("api/products")]
[Authorize]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lists every product in the catalogue.
    /// </summary>
    /// <remarks>This endpoint is anonymous. Products are ordered by identifier.</remarks>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The full product list.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<ProductDto>> result = await mediator.Send(
            new ListAllProductsQuery(),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lists the products created by a specific user.
    /// </summary>
    /// <remarks>
    /// This endpoint is anonymous. The user id is a GUID. A malformed user id does not match
    /// the route and returns 404; a valid but unknown user id also returns 404.
    /// </remarks>
    /// <param name="userId">The user id (GUID) whose products should be returned.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The products owned by the requested user, or an empty list when the user has no products.</response>
    /// <response code="404">The user was not found, or the user id in the route is malformed.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet("by-user/{userId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUser(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<ProductDto>> result = await mediator.Send(
            new ListProductsForUserQuery(userId.ToString()),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Creates a product owned by the authenticated user.
    /// </summary>
    /// <remarks>
    /// The access token identifies the owner; no owner id is accepted in the body. A duplicate
    /// combination of produce date and manufacture email returns 409.
    /// </remarks>
    /// <param name="request">The product data.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="201">The product was created.</response>
    /// <response code="400">The request body is invalid or malformed.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="409">A product with the same produce date and manufacture email already exists.</response>
    /// <response code="415">The request content type is not supported.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesEmptyResponse(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        Result<ProductDto> result = await mediator.Send(
            new CreateProductCommand(
                request.Name,
                request.ManufacturePhone,
                request.ManufactureEmail,
                request.IsAvailable,
                request.ProduceDate),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <remarks>
    /// Requires authentication and ownership. The authenticated user must be the product
    /// creator; otherwise 403 is returned. A missing product returns 404 and a duplicate
    /// produce-date/email combination returns 409.
    /// </remarks>
    /// <param name="id">The product identifier.</param>
    /// <param name="request">The replacement product data.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The product was updated.</response>
    /// <response code="400">The product id or request body is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="403">The authenticated user is not the product creator.</response>
    /// <response code="404">The product was not found.</response>
    /// <response code="409">A product with the same produce date and manufacture email already exists.</response>
    /// <response code="415">The request content type is not supported.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesEmptyResponse(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        Result<ProductDto> result = await mediator.Send(
            new EditProductCommand(
                id,
                request.Name,
                request.ManufacturePhone,
                request.ManufactureEmail,
                request.IsAvailable,
                request.ProduceDate),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Deletes an existing product.
    /// </summary>
    /// <remarks>
    /// Requires authentication and ownership. The authenticated user must be the product
    /// creator; otherwise 403 is returned. A missing product returns 404.
    /// </remarks>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="204">The product was deleted.</response>
    /// <response code="400">The product id is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="403">The authenticated user is not the product creator.</response>
    /// <response code="404">The product was not found.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        Result result = await mediator.Send(new DeleteProductCommand(id), cancellationToken);

        return StatusCode(StatusCodes.Status204NoContent, result);
    }
}
