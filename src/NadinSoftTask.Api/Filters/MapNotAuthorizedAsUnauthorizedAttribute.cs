namespace NadinSoftTask.Api.Filters;

/// <summary>
/// Maps <see cref="NadinSoftTask.Domain.Common.DomainErrorKind.NotAuthorized" /> to HTTP 401
/// instead of HTTP 403 for the annotated controller or action.
/// </summary>
/// <remarks>
/// This is used by authentication endpoints, where invalid credentials are represented by the
/// domain's <c>NotAuthorized</c> error but must be reported as 401. Product ownership failures
/// keep the default 403 mapping.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class MapNotAuthorizedAsUnauthorizedAttribute : Attribute
{
}
