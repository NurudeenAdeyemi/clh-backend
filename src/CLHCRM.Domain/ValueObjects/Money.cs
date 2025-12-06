using CLHCRM.Domain.Common;

namespace CLHCRM.Domain.ValueObjects;

/// <summary>
/// Represents a money value object
/// </summary>
public sealed class Money : ValueObject
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }
    public string Currency { get; }

    public static Result<Money> Create(decimal amount, string? currency = "USD")
    {
        if (amount < 0)
            return Result.Failure<Money>(
                new Error("Money.NegativeAmount", "Money amount cannot be negative"));

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Money>(
                new Error("Money.InvalidCurrency", "Currency cannot be empty"));

        currency = currency.ToUpperInvariant().Trim();

        if (currency.Length != 3)
            return Result.Failure<Money>(
                new Error("Money.InvalidCurrency", "Currency must be a 3-letter ISO code"));

        return Result.Success(new Money(amount, currency));
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");

        if (Amount < other.Amount)
            throw new InvalidOperationException("Result would be negative");

        return new Money(Amount - other.Amount, Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
