namespace KareBooking.V4.Core.DomainModel;

public sealed class KareSchedule(IReadOnlyList<Booking> bookings)
{
    public Result<TimeSlot> ValidateSlot(BookCommand command, DateTime now)
    {
        var slot = TimeSlot.Create(command.Start, command.Hours);
        if (slot.IsFailure) return slot;
        if (now > BookingCalendar.Deadline(slot.Value.Start))
            return Result<TimeSlot>.Failure("Book no later than Friday 16:00 in the previous week.");
        if (bookings.Any(b => !b.IsCancelled && b.Slot.Overlaps(slot.Value)))
            return Result<TimeSlot>.Failure("This slot is already booked.");
        return slot;
    }

    public Result<TimeSlot> ValidateQuoteSlot(BookCommand command, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(command.CustomerEmail) || !command.CustomerEmail.Contains('@'))
            return Result<TimeSlot>.Failure("A customer email is required.");
        return ValidateSlot(command, now);
    }

    public Result<Money> GetQuote(BookCommand command, DateTime now)
    {
        var slot = ValidateQuoteSlot(command, now);
        return slot.IsSuccess ? Result<Money>.Success(Money.ForHours(slot.Value.Hours))
            : Result<Money>.Failure(slot.Error);
    }

    public Result<BookingDecision> Book(BookCommand command, DateTime now, Guid id)
    {
        var slot = ValidateSlot(command, now);
        if (slot.IsFailure) return Result<BookingDecision>.Failure(slot.Error);
        return Booking.Create(id, command.CustomerEmail, slot.Value);
    }
}
