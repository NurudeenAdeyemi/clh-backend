using System.Text.RegularExpressions;
using CLHCRM.Domain.Common;

namespace CLHCRM.Domain.ValueObjects;

/// <summary>
/// Represents an email address value object
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>(
                new Error("Email.Empty", "Email cannot be empty"));

        email = email.Trim().ToLowerInvariant();

        if (email.Length > 255)
            return Result.Failure<Email>(
                new Error("Email.TooLong", "Email cannot be longer than 255 characters"));

        if (!EmailRegex.IsMatch(email))
            return Result.Failure<Email>(
                new Error("Email.InvalidFormat", "Email format is invalid"));

        return Result.Success(new Email(email));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
