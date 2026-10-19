using KareBooking.V4.Core.ApplicationServices;
using KareBooking.V4.Core.DomainModel;
using KareBooking.V4.Api.ImperativeShell.Ports;

namespace KareBooking.V4.Api.ImperativeShell;

public sealed class BookingHandler(IBookingRepository repository, IClock clock, IPaymentGateway payment, BookingEventHandler events)
{
    public async Task<Result<Money>> GetQuoteAsync(BookCommand command)
    {
        var existing = await repository.GetAllAsync();
        var now = clock.Now;
        return BookingService.GetQuote(command, existing, now);
    }

    public async Task<Result<Booking>> BookAsync(BookCommand command)
    {
        var existing = await repository.GetAllAsync();
        var now = clock.Now;
        var id = Guid.NewGuid();
        var decision = BookingService.Book(command, existing, now, id);
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
        var decision = BookingService.Cancel(booking, clock.Now);
        if (decision.IsFailure) return Result<Money>.Failure(decision.Error);
        var refund = decision.Value.Refund;
        await repository.SaveAsync(decision.Value.Booking);
        if (refund.Amount > 0) await payment.RefundAsync(id, refund.Amount);
        await events.DispatchAsync(decision.Value.Events);
        return Result<Money>.Success(refund);
    }
}
