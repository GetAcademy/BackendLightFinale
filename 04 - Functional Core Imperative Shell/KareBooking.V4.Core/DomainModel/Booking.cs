namespace KareBooking.V4.Core.DomainModel;

public sealed record Booking
{
    public Guid Id { get; }
    public string CustomerEmail { get; }
    public TimeSlot Slot { get; }
    public Money Price { get; }
    public bool IsCancelled { get; private init; }
    private Booking(Guid id, string email, TimeSlot slot)
    { Id = id; CustomerEmail = email; Slot = slot; Price = Money.ForHours(slot.Hours); }

    public static Result<BookingDecision> Create(Guid id, string email, TimeSlot slot)
    {
        if (id == Guid.Empty) return Result<BookingDecision>.Failure("Booking id is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Result<BookingDecision>.Failure("A customer email is required.");
        var booking = new Booking(id, email, slot);
        return Result<BookingDecision>.Success(new BookingDecision(booking,
            [new BookingCreated(booking.Id, booking.CustomerEmail, slot.Start, booking.Price)]));
    }

    public Result<CancellationDecision> Cancel(DateTime now)
    {
        if (IsCancelled) return Result<CancellationDecision>.Failure("Booking is already cancelled.");
        var remaining = Slot.Start - now;
        if (remaining <= TimeSpan.Zero) return Result<CancellationDecision>.Failure("Booking has already started.");
        var refund = remaining >= TimeSpan.FromDays(7) ? Price
            : remaining >= TimeSpan.FromHours(48) ? Price.MinusFee(Money.Create(100m).Value) : Money.Zero;
        var cancelled = this with { IsCancelled = true };
        return Result<CancellationDecision>.Success(new CancellationDecision(cancelled, refund,
            [new BookingCancelled(Id, CustomerEmail, refund)]));
    }

    public Booking Copy() => this; // Safe because the complete state is immutable.
}
