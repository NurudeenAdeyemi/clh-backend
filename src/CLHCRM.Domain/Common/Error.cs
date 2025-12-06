namespace CLHCRM.Domain.Common;

/// <summary>
/// Represents an error
/// </summary>
public sealed record Error
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null");

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; }
    public string Message { get; }

    public static Error Validation(string code, string message) =>
        new(code, message);

    public static Error NotFound(string code, string message) =>
        new(code, message);

    public static Error Conflict(string code, string message) =>
        new(code, message);

    public static Error Unauthorized(string code, string message) =>
        new(code, message);

    public static Error Forbidden(string code, string message) =>
        new(code, message);

    public static implicit operator string(Error error) => error.Code;

    public override string ToString() => $"{Code}: {Message}";
}
