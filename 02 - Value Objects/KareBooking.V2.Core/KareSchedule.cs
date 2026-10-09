namespace KareBooking.V2;

public sealed class KareSchedule(IReadOnlyList<Booking> bookings, TimeZoneInfo businessZone)
{
    public Result<TimeSlot> ValidateSlot(BookCommand command, DateTimeOffset now)
    {
        var slot = TimeSlot.Create(command.Start, command.Hours, businessZone);
        if (slot.IsFailure) return slot;
        if (now > BookingCalendar.Deadline(slot.Value.Start, businessZone))
            return Result<TimeSlot>.Failure("Book no later than Friday 16:00 in the previous week.");
        if (bookings.Any(b => !b.IsCancelled && b.Slot.Overlaps(slot.Value)))
            return Result<TimeSlot>.Failure("This slot is already booked.");
        return slot;
    }

    public Result<Money> GetQuote(BookCommand command, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(command.CustomerEmail) || !command.CustomerEmail.Contains('@'))
            return Result<Money>.Failure("A customer email is required.");
        var slot = ValidateSlot(command, now);
        return slot.IsSuccess ? Result<Money>.Success(Money.ForHours(slot.Value.Hours))
            : Result<Money>.Failure(slot.Error);
    }

    public Result<Booking> Book(BookCommand command, DateTimeOffset now, Guid id)
    {
        var slot = ValidateSlot(command, now);
        if (slot.IsFailure) return Result<Booking>.Failure(slot.Error);
        return Booking.Create(id, command.CustomerEmail, slot.Value);
    }
}
