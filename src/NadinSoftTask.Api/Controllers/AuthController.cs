using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NadinSoftTask.Api.Contracts.Auth;
using NadinSoftTask.Api.Filters;
using NadinSoftTask.Application.Commands.Login;
using NadinSoftTask.Application.Commands.RefreshTokens;
using NadinSoftTask.Application.Commands.RegisterUser;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Api.Controllers;

/// <summary>
/// Authentication endpoints for registration, login, and token refresh.
/// </summary>
/// <remarks>
/// All endpoints in this controller are anonymous. Successful calls return an access token and
/// a refresh token. Use the access token with the Swagger Authorize dialog or send it as
/// <c>Authorization: Bearer {accessToken}</c>.
/// </remarks>
[ApiController]
[Route("api/auth")]
[Consumes("application/json")]
[AllowAnonymous]
[MapNotAuthorizedAsUnauthorized]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a new user account and immediately issues an access/refresh token pair.
    /// </summary>
    /// <remarks>
    /// Usernames must be 3-50 characters long and may contain only letters, numbers, and
    /// underscores. Passwords must be 6-100 characters long. A duplicate username returns 409.
    /// </remarks>
    /// <param name="request">The new account credentials.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The account was created and tokens were issued.</response>
    /// <response code="400">The request body is invalid or malformed.</response>
    /// <response code="409">The username is already taken.</response>
    /// <response code="415">The request content type is not supported.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        Result<AuthResult> result = await mediator.Send(
            new RegisterUserCommand(request.Username, request.Password),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Authenticates an existing user and issues an access/refresh token pair.
    /// </summary>
    /// <remarks>
    /// Returns 401 when the username or password is invalid. Use the returned access token with
    /// the Swagger Authorize dialog or send it as <c>Authorization: Bearer {accessToken}</c>.
    /// </remarks>
    /// <param name="request">The login credentials.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The credentials are valid and tokens were issued.</response>
    /// <response code="400">The request body is invalid or malformed.</response>
    /// <response code="401">The username or password is invalid.</response>
    /// <response code="415">The request content type is not supported.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        Result<AuthResult> result = await mediator.Send(
            new LoginCommand(request.Username, request.Password),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Rotates a refresh token and issues a new access/refresh token pair.
    /// </summary>
    /// <remarks>
    /// The supplied refresh token is revoked. A missing, expired, revoked, or otherwise invalid
    /// refresh token returns 401.
    /// </remarks>
    /// <param name="request">The refresh token to rotate.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The refresh token was valid and a new token pair was issued.</response>
    /// <response code="400">The request body is invalid or malformed.</response>
    /// <response code="401">The refresh token is missing, expired, revoked, or invalid.</response>
    /// <response code="415">The request content type is not supported.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        Result<AuthResult> result = await mediator.Send(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        return Ok(result);
    }
}
