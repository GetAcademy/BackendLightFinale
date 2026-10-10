namespace KareBooking.V0;

public sealed class BookingService(
    IBookingRepository repository, IClock clock, IEmailSender email, IPaymentGateway payment,
    IAuditLog audit, TimeZoneInfo businessZone)
{
    public Task<Result<decimal>> GetQuoteAsync(BookCommand command) => EvaluateBookingAsync(command);

    private async Task<Result<decimal>> EvaluateBookingAsync(BookCommand command)
    {
        var local = TimeZoneInfo.ConvertTime(command.Start, businessZone);
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
        if (clock.Now > BookingCalendar.Deadline(local, businessZone))
            return Result<decimal>.Failure("Book no later than Friday 16:00 in the previous week.");
        var existing = await repository.GetAllAsync();
        if (existing.Any(b => !b.IsCancelled && local < b.End && local.AddHours(command.Hours) > b.Start))
            return Result<decimal>.Failure("This slot is already booked.");
        var price = 500m + (command.Hours - 1) * 300m;
        return Result<decimal>.Success(price);
    }

    public async Task<Result<Booking>> BookAsync(BookCommand command)
    {
        var quote = await EvaluateBookingAsync(command);
        if (quote.IsFailure) return Result<Booking>.Failure(quote.Error);
        var local = TimeZoneInfo.ConvertTime(command.Start, businessZone);
        var price = quote.Value;
        var booking = new Booking(Guid.NewGuid(), command.CustomerEmail, local, command.Hours, price);
        await repository.SaveAsync(booking);
        await payment.ChargeAsync(booking.Id, price);
        await email.SendAsync(booking.CustomerEmail, $"Booking {booking.Id} confirmed: {price} NOK");
        await audit.WriteAsync($"BookingCreated {booking.Id}");
        return Result<Booking>.Success(booking);
    }

    public async Task<Result<decimal>> CancelAsync(Guid id)
    {
        var booking = await repository.GetByIdAsync(id);
        if (booking is null) return Result<decimal>.Failure("Booking not found.");
        if (booking.IsCancelled) return Result<decimal>.Failure("Booking is already cancelled.");
        var remaining = booking.Start - clock.Now;
        if (remaining <= TimeSpan.Zero) return Result<decimal>.Failure("Booking has already started.");
        var refund = remaining >= TimeSpan.FromDays(7) ? booking.Price
            : remaining >= TimeSpan.FromHours(48) ? Math.Max(0, booking.Price - 100m) : 0m;
        booking.IsCancelled = true;
        await repository.SaveAsync(booking);
        if (refund > 0) await payment.RefundAsync(booking.Id, refund);
        await email.SendAsync(booking.CustomerEmail, $"Booking {booking.Id} cancelled; refund {refund} NOK");
        await audit.WriteAsync($"BookingCancelled {booking.Id}");
        return Result<decimal>.Success(refund);
    }
}
