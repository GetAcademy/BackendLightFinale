using KareBooking.V4.DomainModel;
using KareBooking.V4.DomainServices;

namespace KareBooking.V4.ApplicationServices;

public sealed class BookingService(
    IBookingRepository repository, IClock clock, IPaymentGateway payment, BookingEventHandler events)
{
    public async Task<Result<Money>> GetQuoteAsync(BookCommand command)
    {
        var existing = await repository.GetAllAsync();
        var now = clock.Now;
        return BookingWorkflow.GetQuote(command, existing, now);
    }

    public async Task<Result<Booking>> BookAsync(BookCommand command)
    {
        var existing = await repository.GetAllAsync();
        var now = clock.Now;
        var id = Guid.NewGuid();
        var decision = BookingWorkflow.Book(command, existing, now, id);
        if (decision.IsFailure) return Result<Booking>.Failure(decision.Error);
        var booking = decision.Value.Booking;
        await repository.SaveAsync(booking);
        await payment.ChargeAsync(booking.Id, booking.Price.Amount);
        await events.DispatchAsync(decision.Value.Events);
        return Result<Booking>.Success(booking);
    }

    public async Task<Result<Money>> CancelAsync(Guid id)
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
}
