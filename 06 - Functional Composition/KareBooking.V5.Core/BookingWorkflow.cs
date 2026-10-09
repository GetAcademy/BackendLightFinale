namespace KareBooking.V5;

public static class BookingWorkflow
{
    public static Result<BookingDecision> Book(BookCommand command, IReadOnlyList<Booking> existing,
        DateTimeOffset now, Guid id, TimeZoneInfo businessZone)
    {
        return TimeSlot.Create(command.Start, command.Hours, businessZone)
            .Bind(slot => ValidateAvailability(slot, existing, now, businessZone))
            .Bind(slot => Booking.Create(id, command.CustomerEmail, slot))
            .Map(ToDecision);
    }

    public static Result<Money> GetQuote(BookCommand command, IReadOnlyList<Booking> existing,
        DateTimeOffset now, TimeZoneInfo businessZone)
    {
        return TimeSlot.Create(command.Start, command.Hours, businessZone)
            .Bind(slot => ValidateAvailability(slot, existing, now, businessZone))
            .Bind(slot => string.IsNullOrWhiteSpace(command.CustomerEmail) || !command.CustomerEmail.Contains('@')
                ? Result<TimeSlot>.Failure("A customer email is required.") : Result<TimeSlot>.Success(slot))
            .Map(slot => Money.ForHours(slot.Hours));
    }

    public static Result<TimeSlot> ValidateAvailability(TimeSlot slot, IReadOnlyList<Booking> existing,
        DateTimeOffset now, TimeZoneInfo businessZone)
    {
        if (now > BookingCalendar.Deadline(slot.Start, businessZone))
            return Result<TimeSlot>.Failure("Book no later than Friday 16:00 in the previous week.");
        if (existing.Any(b => !b.IsCancelled && b.Slot.Overlaps(slot)))
            return Result<TimeSlot>.Failure("This slot is already booked.");
        return Result<TimeSlot>.Success(slot);
    }

    private static BookingDecision ToDecision(Booking booking) => new(booking,
        [new BookingCreated(booking.Id, booking.CustomerEmail, booking.Slot.Start, booking.Price)]);

    public static Result<CancellationDecision> Cancel(Booking booking, DateTimeOffset now)
    {
        if (booking.IsCancelled) return Result<CancellationDecision>.Failure("Booking is already cancelled.");
        var remaining = booking.Slot.Start - now;
        if (remaining <= TimeSpan.Zero) return Result<CancellationDecision>.Failure("Booking has already started.");
        var refund = remaining >= TimeSpan.FromDays(7) ? booking.Price
            : remaining >= TimeSpan.FromHours(48) ? booking.Price.MinusFee(Money.Create(100m).Value) : Money.Zero;
        return Result<CancellationDecision>.Success(new CancellationDecision(booking.AsCancelled(), refund,
            [new BookingCancelled(booking.Id, booking.CustomerEmail, refund)]));
    }
}
