using System.Text.RegularExpressions;
using CLHCRM.Domain.Common;

namespace CLHCRM.Domain.ValueObjects;

/// <summary>
/// Represents a phone number value object
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PhoneNumber> Create(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result.Failure<PhoneNumber>(
                new Error("PhoneNumber.Empty", "Phone number cannot be empty"));

        // Remove common separators
        phoneNumber = phoneNumber.Replace(" ", "")
                                .Replace("-", "")
                                .Replace("(", "")
                                .Replace(")", "")
                                .Trim();

        if (!PhoneRegex.IsMatch(phoneNumber))
            return Result.Failure<PhoneNumber>(
                new Error("PhoneNumber.InvalidFormat", "Phone number format is invalid"));

        return Result.Success(new PhoneNumber(phoneNumber));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
