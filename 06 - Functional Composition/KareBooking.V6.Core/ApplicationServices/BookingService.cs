using KareBooking.V6.Core.DomainModel;

namespace KareBooking.V6.Core.ApplicationServices;

public static class BookingService
{
    public static Result<BookingDecision> Book(BookCommand command, IReadOnlyList<Booking> existing,
        DateTime now, Guid id)
    {
        var schedule = new KareSchedule(existing);
        return schedule.ValidateSlot(command, now)
            .FlatMap(slot => Booking.Create(id, command.CustomerEmail, slot));
    }

    public static Result<Money> GetQuote(BookCommand command, IReadOnlyList<Booking> existing,
        DateTime now)
    {
        var schedule = new KareSchedule(existing);
        return schedule.ValidateQuoteSlot(command, now)
            .Map(slot => Money.ForHours(slot.Hours));
    }

    public static Result<CancellationDecision> Cancel(Booking booking, DateTime now) =>
        booking.Cancel(now);
}
