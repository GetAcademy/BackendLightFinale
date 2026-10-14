namespace KareBooking.V1.Core.DomainModel;

public sealed class KareSchedule(IReadOnlyList<Booking> bookings)
{
    public Result<decimal> GetQuote(BookCommand command, DateTime now)
    {
        var local = command.Start;
        if (string.IsNullOrWhiteSpace(command.CustomerEmail) || !command.CustomerEmail.Contains('@'))
            return Result<decimal>.Failure("A customer email is required.");
        if (local.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return Result<decimal>.Failure("Kare works Monday to Friday.");
        if (local.Minute != 0 || local.Second != 0 || local.Ticks % TimeSpan.TicksPerSecond != 0)
            return Result<decimal>.Failure("Start must be a whole clock hour.");
        if (command.Hours is < 1 or > 4)
            return Result<decimal>.Failure("Duration must be 1 to 4 whole hours.");
        if (local.Hour < 12 || local.Hour + command.Hours > 16)
            return Result<decimal>.Failure("The booking must fit within 12:00–16:00.");
        if (now > BookingCalendar.Deadline(local))
            return Result<decimal>.Failure("Book no later than Friday 16:00 in the previous week.");
        if (bookings.Any(b => b.Overlaps(local, command.Hours)))
            return Result<decimal>.Failure("This slot is already booked.");
        return Result<decimal>.Success(500m + (command.Hours - 1) * 300m);
    }

    public Result<Booking> Book(BookCommand command, DateTime now, Guid id)
    {
        var quote = GetQuote(command, now);
        if (quote.IsFailure) return Result<Booking>.Failure(quote.Error);
        return Result<Booking>.Success(new Booking(id, command.CustomerEmail,
            command.Start, command.Hours, quote.Value));
    }
}
