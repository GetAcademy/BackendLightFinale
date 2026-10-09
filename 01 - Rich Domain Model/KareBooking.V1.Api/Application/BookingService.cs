namespace KareBooking.V1;

public sealed class BookingService(
    IBookingRepository repository, IClock clock, IPaymentGateway payment, IEmailSender email, IAuditLog audit, TimeZoneInfo businessZone)
{
    // Single process teaching host: serialize read-decide-save to prevent double booking.
    private readonly SemaphoreSlim gate = new(1, 1);

    public async Task<Result<decimal>> GetQuoteAsync(BookCommand command)
    {
        var existing = await repository.GetAllAsync();
        var now = clock.Now;
        return new KareSchedule(existing, businessZone).GetQuote(command, now);
    }

    public async Task<Result<Booking>> BookAsync(BookCommand command)
    {
        await gate.WaitAsync();
        try
        {
            var existing = await repository.GetAllAsync();
            var now = clock.Now;
            var id = Guid.NewGuid();
            var decision = new KareSchedule(existing, businessZone).Book(command, now, id);
            if (decision.IsFailure) return Result<Booking>.Failure(decision.Error);
            var booking = decision.Value;
            await repository.SaveAsync(booking);
            await payment.ChargeAsync(booking.Id, booking.Price);
            await email.SendAsync(booking.CustomerEmail, $"Booking {booking.Id} confirmed");
            await audit.WriteAsync($"BookingCreated {booking.Id}");
            return Result<Booking>.Success(booking);
        }
        finally { gate.Release(); }
    }

    public async Task<Result<decimal>> CancelAsync(Guid id)
    {
        await gate.WaitAsync();
        try
        {
            var booking = await repository.GetByIdAsync(id);
            if (booking is null) return Result<decimal>.Failure("Booking not found.");
            var decision = booking.Cancel(clock.Now);
            if (decision.IsFailure) return Result<decimal>.Failure(decision.Error);
            var refund = decision.Value;
            await repository.SaveAsync(booking);
            if (refund > 0) await payment.RefundAsync(id, refund);
            await email.SendAsync(booking.CustomerEmail, $"Booking {booking.Id} cancelled; refund {refund} NOK");
            await audit.WriteAsync($"BookingCancelled {booking.Id}");
            return Result<decimal>.Success(refund);
        }
        finally { gate.Release(); }
    }
}
