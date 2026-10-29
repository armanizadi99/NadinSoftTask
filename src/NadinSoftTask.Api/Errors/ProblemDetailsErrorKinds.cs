namespace NadinSoftTask.Api.Errors;

public static class ProblemDetailsErrorKinds
{
    public const string Validation = "Validation";

    public const string BadRequest = "BadRequest";

    public const string Unauthorized = "Unauthorized";

    public const string Forbidden = "Forbidden";

    public const string NotFound = "NotFound";

    public const string Conflict = "Conflict";

    public const string MethodNotAllowed = "MethodNotAllowed";

    public const string InternalServerError = "InternalServerError";
}

public static class ProblemDetailsDefaults
{
    public const string ContentType = "application/problem+json";

    public const string Type = "about:blank";
}
