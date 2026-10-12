using KareBooking.V2.DomainModel;
using KareBooking.V2.DomainServices;

namespace KareBooking.V2.ApplicationServices;

public sealed class BookingService(
    IBookingRepository repository, IClock clock, IPaymentGateway payment, IEmailSender email, IAuditLog audit)
{
    public async Task<Result<Money>> GetQuoteAsync(BookCommand command)
    {
        var existing = await repository.GetAllAsync();
        var now = clock.Now;
        return new KareSchedule(existing).GetQuote(command, now);
    }

    public async Task<Result<Booking>> BookAsync(BookCommand command)
    {
        var existing = await repository.GetAllAsync();
        var now = clock.Now;
        var id = Guid.NewGuid();
        var decision = new KareSchedule(existing).Book(command, now, id);
        if (decision.IsFailure) return Result<Booking>.Failure(decision.Error);
        var booking = decision.Value;
        await repository.SaveAsync(booking);
        await payment.ChargeAsync(booking.Id, booking.Price.Amount);
        await email.SendAsync(booking.CustomerEmail, $"Booking {booking.Id} confirmed");
        await audit.WriteAsync($"BookingCreated {booking.Id}");
        return Result<Booking>.Success(booking);
    }

    public async Task<Result<Money>> CancelAsync(Guid id)
    {
        var booking = await repository.GetByIdAsync(id);
        if (booking is null) return Result<Money>.Failure("Booking not found.");
        var decision = booking.Cancel(clock.Now);
        if (decision.IsFailure) return Result<Money>.Failure(decision.Error);
        var refund = decision.Value;
        await repository.SaveAsync(booking);
        if (refund.Amount > 0) await payment.RefundAsync(id, refund.Amount);
        await email.SendAsync(booking.CustomerEmail, $"Booking {booking.Id} cancelled; refund {refund.Amount} NOK");
        await audit.WriteAsync($"BookingCancelled {booking.Id}");
        return Result<Money>.Success(refund);
    }
}
