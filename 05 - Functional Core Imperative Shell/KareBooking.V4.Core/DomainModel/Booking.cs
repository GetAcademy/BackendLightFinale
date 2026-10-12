namespace KareBooking.V4.DomainModel;

public sealed record Booking
{
    public Guid Id { get; }
    public string CustomerEmail { get; }
    public TimeSlot Slot { get; }
    public Money Price { get; }
    public bool IsCancelled { get; private init; }
    private Booking(Guid id, string email, TimeSlot slot)
    { Id = id; CustomerEmail = email; Slot = slot; Price = Money.ForHours(slot.Hours); }

    public static Result<Booking> Create(Guid id, string email, TimeSlot slot)
    {
        if (id == Guid.Empty) return Result<Booking>.Failure("Booking id is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Result<Booking>.Failure("A customer email is required.");
        return Result<Booking>.Success(new Booking(id, email, slot));
    }

    public Booking AsCancelled() => this with { IsCancelled = true };
    public Booking Copy() => this; // Safe because the complete state is immutable.
}
