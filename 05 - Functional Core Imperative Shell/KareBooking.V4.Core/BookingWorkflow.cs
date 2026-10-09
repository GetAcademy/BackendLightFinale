namespace KareBooking.V4;

public static class BookingWorkflow
{
    public static Result<BookingDecision> Book(BookCommand command, IReadOnlyList<Booking> existing,
        DateTimeOffset now, Guid id, TimeZoneInfo businessZone)
    {
        var slot = TimeSlot.Create(command.Start, command.Hours, businessZone);
        if (slot.IsFailure) return Result<BookingDecision>.Failure(slot.Error);
        var available = ValidateAvailability(slot.Value, existing, now, businessZone);
        if (available.IsFailure) return Result<BookingDecision>.Failure(available.Error);
        var booking = Booking.Create(id, command.CustomerEmail, available.Value);
        if (booking.IsFailure) return Result<BookingDecision>.Failure(booking.Error);
        return Result<BookingDecision>.Success(ToDecision(booking.Value));
    }

    public static Result<Money> GetQuote(BookCommand command, IReadOnlyList<Booking> existing,
        DateTimeOffset now, TimeZoneInfo businessZone)
    {
        if (string.IsNullOrWhiteSpace(command.CustomerEmail) || !command.CustomerEmail.Contains('@'))
            return Result<Money>.Failure("A customer email is required.");
        var slot = TimeSlot.Create(command.Start, command.Hours, businessZone);
        if (slot.IsFailure) return Result<Money>.Failure(slot.Error);
        var available = ValidateAvailability(slot.Value, existing, now, businessZone);
        return available.IsSuccess ? Result<Money>.Success(Money.ForHours(available.Value.Hours))
            : Result<Money>.Failure(available.Error);
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
