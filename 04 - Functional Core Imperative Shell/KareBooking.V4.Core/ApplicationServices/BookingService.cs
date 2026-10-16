using KareBooking.V4.Core.DomainModel;

namespace KareBooking.V4.Core.ApplicationServices;

public static class BookingService
{
    public static Result<BookingDecision> Book(BookCommand command, IReadOnlyList<Booking> existing,
        DateTime now, Guid id)
    {
        var schedule = new KareSchedule(existing);
        var slot = schedule.ValidateSlot(command, now);
        if (slot.IsFailure) return Result<BookingDecision>.Failure(slot.Error);
        return Booking.Create(id, command.CustomerEmail, slot.Value);
    }

    public static Result<Money> GetQuote(BookCommand command, IReadOnlyList<Booking> existing,
        DateTime now)
    {
        var schedule = new KareSchedule(existing);
        return schedule.GetQuote(command, now);
    }

    public static Result<CancellationDecision> Cancel(Booking booking, DateTime now) =>
        booking.Cancel(now);
}
