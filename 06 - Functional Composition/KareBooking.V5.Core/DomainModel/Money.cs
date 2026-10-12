namespace KareBooking.V5.DomainModel;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency => "NOK";
    private Money(decimal amount) => Amount = amount;

    public static Result<Money> Create(decimal amount) => amount < 0 || decimal.Round(amount, 2) != amount
        ? Result<Money>.Failure("Money must be non-negative with at most two decimals.")
        : Result<Money>.Success(new Money(amount));

    public static Money ForHours(int hours)
    {
        if (hours is < 1 or > 4) throw new ArgumentOutOfRangeException(nameof(hours));
        return new Money(500m + (hours - 1) * 300m);
    }

    public Money MinusFee(Money fee) => new(Math.Max(0, Amount - fee.Amount));
    public static Money Zero => new(0);
}
