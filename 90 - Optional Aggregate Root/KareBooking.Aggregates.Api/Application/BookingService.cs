namespace KareBooking.Aggregates;

public sealed class BookingService(IScheduleRepository repository, IClock clock,
    IPaymentGateway payment, IEmailSender email, IAuditLog audit)
{
    public async Task<Result<Money>> GetQuoteAsync(BookCommand command) =>
        (await repository.LoadAsync()).Schedule.GetQuote(command, clock.Now);

    public async Task<Result<Booking>> BookAsync(BookCommand command)
    {
        var snapshot = await repository.LoadAsync();
        var result = snapshot.Schedule.Book(command, clock.Now, Guid.NewGuid());
        if (result.IsFailure) return result;
        if (!await repository.TrySaveAsync(snapshot.Schedule, snapshot.Version))
            return Result<Booking>.Failure("Schedule changed; request a new quote and try again.");
        await payment.ChargeAsync(result.Value.Id, result.Value.Price.Amount);
        await email.SendAsync(result.Value.CustomerEmail, $"Booking {result.Value.Id} confirmed");
        await audit.WriteAsync($"BookingCreated {result.Value.Id}");
        return result;
    }

    public async Task<Result<Money>> CancelAsync(Guid id)
    {
        var snapshot = await repository.LoadAsync();
        var result = snapshot.Schedule.Cancel(id, clock.Now);
        if (result.IsFailure) return result;
        if (!await repository.TrySaveAsync(snapshot.Schedule, snapshot.Version))
            return Result<Money>.Failure("Schedule changed; try again.");
        var booking = snapshot.Schedule.Bookings.Single(b => b.Id == id);
        if (result.Value.Amount > 0) await payment.RefundAsync(id, result.Value.Amount);
        await email.SendAsync(booking.CustomerEmail, $"Booking {id} cancelled");
        await audit.WriteAsync($"BookingCancelled {id}");
        return result;
    }
}
