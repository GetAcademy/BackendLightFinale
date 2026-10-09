namespace KareBooking.V3;

public sealed class Booking
{
    public Guid Id { get; }
    public string CustomerEmail { get; }
    public TimeSlot Slot { get; }
    public Money Price { get; }
    public bool IsCancelled { get; private set; }
    private Booking(Guid id, string email, TimeSlot slot, Money price)
    { Id = id; CustomerEmail = email; Slot = slot; Price = price; }

    public static Result<BookingDecision> Create(Guid id, string email, TimeSlot slot)
    {
        if (id == Guid.Empty) return Result<BookingDecision>.Failure("Booking id is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Result<BookingDecision>.Failure("A customer email is required.");
        var booking = new Booking(id, email, slot, Money.ForHours(slot.Hours));
        return Result<BookingDecision>.Success(new BookingDecision(booking,
            [new BookingCreated(booking.Id, booking.CustomerEmail, slot.Start, booking.Price)]));
    }

    public Result<CancellationDecision> Cancel(DateTimeOffset now)
    {
        if (IsCancelled) return Result<CancellationDecision>.Failure("Booking is already cancelled.");
        var remaining = Slot.Start - now;
        if (remaining <= TimeSpan.Zero) return Result<CancellationDecision>.Failure("Booking has already started.");
        var refund = remaining >= TimeSpan.FromDays(7) ? Price
            : remaining >= TimeSpan.FromHours(48) ? Price.MinusFee(Money.Create(100m).Value) : Money.Zero;
        IsCancelled = true;
        return Result<CancellationDecision>.Success(new CancellationDecision(this, refund,
            [new BookingCancelled(Id, CustomerEmail, refund)]));
    }

    public Booking Copy() => new(Id, CustomerEmail, Slot, Price) { IsCancelled = IsCancelled };
}
