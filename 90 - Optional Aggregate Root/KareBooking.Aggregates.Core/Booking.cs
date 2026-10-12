namespace KareBooking.Aggregates;

public sealed class Booking
{
    public Guid Id { get; }
    public string CustomerEmail { get; }
    public TimeSlot Slot { get; }
    public Money Price { get; }
    public bool IsCancelled { get; private set; }
    private Booking(Guid id, string email, TimeSlot slot, Money price)
    { Id = id; CustomerEmail = email; Slot = slot; Price = price; }

    internal static Result<Booking> Create(Guid id, string email, TimeSlot slot)
    {
        if (id == Guid.Empty) return Result<Booking>.Failure("Booking id is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Result<Booking>.Failure("A customer email is required.");
        var booking = new Booking(id, email, slot, Money.ForHours(slot.Hours));
        return Result<Booking>.Success(booking);
    }

    internal Result<Money> Cancel(DateTime now)
    {
        if (IsCancelled) return Result<Money>.Failure("Booking is already cancelled.");
        var remaining = Slot.Start - now;
        if (remaining <= TimeSpan.Zero) return Result<Money>.Failure("Booking has already started.");
        var refund = remaining >= TimeSpan.FromDays(7) ? Price
            : remaining >= TimeSpan.FromHours(48) ? Price.MinusFee(Money.Create(100m).Value) : Money.Zero;
        IsCancelled = true;
        return Result<Money>.Success(refund);
    }

    public Booking Copy() => new(Id, CustomerEmail, Slot, Price) { IsCancelled = IsCancelled };
}
