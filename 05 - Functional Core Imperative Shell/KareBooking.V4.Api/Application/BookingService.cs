namespace KareBooking.V4;

public sealed class BookingService(
    IBookingRepository repository, IClock clock, IPaymentGateway payment, BookingEventHandler events, TimeZoneInfo businessZone)
{
    // Single process teaching host: serialize read-decide-save to prevent double booking.
    private readonly SemaphoreSlim gate = new(1, 1);

    public async Task<Result<Money>> GetQuoteAsync(BookCommand command)
    {
        var existing = await repository.GetAllAsync();
        var now = clock.Now;
        return BookingWorkflow.GetQuote(command, existing, now, businessZone);
    }

    public async Task<Result<Booking>> BookAsync(BookCommand command)
    {
        await gate.WaitAsync();
        try
        {
            var existing = await repository.GetAllAsync();
            var now = clock.Now;
            var id = Guid.NewGuid();
            var decision = BookingWorkflow.Book(command, existing, now, id, businessZone);
            if (decision.IsFailure) return Result<Booking>.Failure(decision.Error);
            var booking = decision.Value.Booking;
            await repository.SaveAsync(booking);
            await payment.ChargeAsync(booking.Id, booking.Price.Amount);
            await events.DispatchAsync(decision.Value.Events);
            return Result<Booking>.Success(booking);
        }
        finally { gate.Release(); }
    }

    public async Task<Result<Money>> CancelAsync(Guid id)
    {
        await gate.WaitAsync();
        try
        {
            var booking = await repository.GetByIdAsync(id);
            if (booking is null) return Result<Money>.Failure("Booking not found.");
            var decision = BookingWorkflow.Cancel(booking, clock.Now);
            if (decision.IsFailure) return Result<Money>.Failure(decision.Error);
            var refund = decision.Value.Refund;
            await repository.SaveAsync(decision.Value.Booking);
            if (refund.Amount > 0) await payment.RefundAsync(id, refund.Amount);
            await events.DispatchAsync(decision.Value.Events);
            return Result<Money>.Success(refund);
        }
        finally { gate.Release(); }
    }
}
