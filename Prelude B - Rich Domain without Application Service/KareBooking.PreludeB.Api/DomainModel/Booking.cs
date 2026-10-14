namespace KareBooking.PreludeB.Api.DomainModel;

public sealed class Booking
{
    public Guid Id { get; }
    public string CustomerEmail { get; }
    public DateTime Start { get; }
    public int Hours { get; }
    public DateTime End => Start.AddHours(Hours);
    public decimal Price { get; }
    public bool IsCancelled { get; private set; }

    internal Booking(Guid id, string customerEmail, DateTime start, int hours, decimal price)
    {
        Id = id; 
        CustomerEmail = customerEmail; 
        Start = start; 
        Hours = hours; 
        Price = price;
    }

    public bool Overlaps(DateTime start, int hours)
    {
        if (IsCancelled) return false;
        var end = start.AddHours(hours);
        var endsBeforeOrAtStart = end <= Start;
        var startsAfterOrAtEnd = start >= End;
        return !(endsBeforeOrAtStart || startsAfterOrAtEnd);
    }

    public Result<decimal> Cancel(DateTime now)
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


