namespace KareBooking.Aggregates.Core;

public sealed class KareSchedule
{
    private readonly List<Booking> bookings;
    public IReadOnlyList<Booking> Bookings => bookings.Select(b => b.Copy()).ToList().AsReadOnly();

    public KareSchedule() : this([]) { }
    private KareSchedule(IEnumerable<Booking> bookings)
    { this.bookings = bookings.Select(b => b.Copy()).ToList(); }

    public Result<Money> GetQuote(BookCommand command, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(command.CustomerEmail) || !command.CustomerEmail.Contains('@'))
            return Result<Money>.Failure("A customer email is required.");
        var slot = ValidateSlot(command, now);
        return slot.IsSuccess ? Result<Money>.Success(Money.ForHours(slot.Value.Hours))
            : Result<Money>.Failure(slot.Error);
    }

    private Result<TimeSlot> ValidateSlot(BookCommand command, DateTime now)
    {
        var slot = TimeSlot.Create(command.Start, command.Hours);
        if (slot.IsFailure) return slot;
        if (now > BookingCalendar.Deadline(slot.Value.Start))
            return Result<TimeSlot>.Failure("Book no later than Friday 16:00 in the previous week.");
        if (bookings.Any(b => !b.IsCancelled && b.Slot.Overlaps(slot.Value)))
            return Result<TimeSlot>.Failure("This slot is already booked.");
        return slot;
    }

    public Result<Booking> Book(BookCommand command, DateTime now, Guid id)
    {
        if (bookings.Any(b => b.Id == id)) return Result<Booking>.Failure("Booking id already exists.");
        var slot = ValidateSlot(command, now);
        if (slot.IsFailure) return Result<Booking>.Failure(slot.Error);
        var booking = Booking.Create(id, command.CustomerEmail, slot.Value);
        if (booking.IsSuccess) bookings.Add(booking.Value);
        return booking.IsSuccess ? Result<Booking>.Success(booking.Value.Copy()) : booking;
    }

    public Result<Money> Cancel(Guid id, DateTime now)
    {
        var booking = bookings.Find(b => b.Id == id);
        return booking is null ? Result<Money>.Failure("Booking not found.") : booking.Cancel(now);
    }

    public KareSchedule Copy() => new(bookings);
}
