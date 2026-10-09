namespace KareBooking.V5;

public abstract record DomainEvent;
public sealed record BookingCreated(Guid BookingId, string CustomerEmail, DateTimeOffset Start, Money Price)
    : DomainEvent;
public sealed record BookingCancelled(Guid BookingId, string CustomerEmail, Money Refund) : DomainEvent;
public sealed record BookingDecision(Booking Booking, IReadOnlyList<DomainEvent> Events);
public sealed record CancellationDecision(Booking Booking, Money Refund, IReadOnlyList<DomainEvent> Events);
