namespace KareBooking.V1;

public sealed class Booking
{
    public Guid Id { get; }
    public string CustomerEmail { get; }
    public DateTimeOffset Start { get; }
    public int Hours { get; }
    public DateTimeOffset End => Start.AddHours(Hours);
    public decimal Price { get; }
    public bool IsCancelled { get; private set; }

    internal Booking(Guid id, string customerEmail, DateTimeOffset start, int hours, decimal price)
    { Id = id; CustomerEmail = customerEmail; Start = start; Hours = hours; Price = price; }

    public bool Overlaps(DateTimeOffset start, int hours) => !IsCancelled && start < End && start.AddHours(hours) > Start;

    public Result<decimal> Cancel(DateTimeOffset now)
    {
        if (IsCancelled) return Result<decimal>.Failure("Booking is already cancelled.");
        var remaining = Start - now;
        if (remaining <= TimeSpan.Zero) return Result<decimal>.Failure("Booking has already started.");
        var refund = remaining >= TimeSpan.FromDays(7) ? Price
            : remaining >= TimeSpan.FromHours(48) ? Math.Max(0, Price - 100m) : 0m;
        IsCancelled = true;
        return Result<decimal>.Success(refund);
    }

    public Booking Copy() => new(Id, CustomerEmail, Start, Hours, Price) { IsCancelled = IsCancelled };
}
