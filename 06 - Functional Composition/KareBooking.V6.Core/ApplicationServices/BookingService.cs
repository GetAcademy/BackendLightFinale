using KareBooking.V6.Core.DomainModel;

namespace KareBooking.V6.Core.ApplicationServices;

public static class BookingService
{
    public static Result<BookingDecision> Book(BookCommand command, IReadOnlyList<Booking> existing,
        DateTime now, Guid id)
    {
        return TimeSlot.Create(command.Start, command.Hours)
            .FlatMap(slot => ValidateAvailability(slot, existing, now))
            .FlatMap(slot => Booking.Create(id, command.CustomerEmail, slot))
            .Map(ToDecision);
    }

    public static Result<Money> GetQuote(BookCommand command, IReadOnlyList<Booking> existing,
        DateTime now)
    {
        if (string.IsNullOrWhiteSpace(command.CustomerEmail) || !command.CustomerEmail.Contains('@'))
            return Result<Money>.Failure("A customer email is required.");
        return TimeSlot.Create(command.Start, command.Hours)
            .FlatMap(slot => ValidateAvailability(slot, existing, now))
            .Map(slot => Money.ForHours(slot.Hours));
    }

    public static Result<TimeSlot> ValidateAvailability(TimeSlot slot, IReadOnlyList<Booking> existing,
        DateTime now)
    {
        if (now > BookingCalendar.Deadline(slot.Start))
            return Result<TimeSlot>.Failure("Book no later than Friday 16:00 in the previous week.");
        if (existing.Any(b => !b.IsCancelled && b.Slot.Overlaps(slot)))
            return Result<TimeSlot>.Failure("This slot is already booked.");
        return Result<TimeSlot>.Success(slot);
    }

    private static BookingDecision ToDecision(Booking booking) => new(booking,
        [new BookingCreated(booking.Id, booking.CustomerEmail, booking.Slot.Start, booking.Price)]);

    public static Result<CancellationDecision> Cancel(Booking booking, DateTime now)
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
