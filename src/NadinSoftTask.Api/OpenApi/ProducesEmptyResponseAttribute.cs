namespace NadinSoftTask.Api.OpenApi;

/// <summary>
/// Documents a response status code that has no response body, overriding Swashbuckle's
/// automatic problem-details schema for error responses.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class ProducesEmptyResponseAttribute(int statusCode) : Attribute
{
    /// <summary>
    /// The HTTP status code that has no response body.
    /// </summary>
    public int StatusCode { get; } = statusCode;
}
