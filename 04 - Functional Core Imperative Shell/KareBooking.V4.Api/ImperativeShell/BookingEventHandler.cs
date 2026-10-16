using KareBooking.V4.Core.DomainModel;
using KareBooking.V4.Api.ImperativeShell.Ports;

namespace KareBooking.V4.Api.ImperativeShell;

public sealed class BookingEventHandler(IEmailSender email, IAuditLog audit)
{
    public async Task DispatchAsync(IReadOnlyList<DomainEvent> events)
    {
        foreach (var fact in events)
        {
            switch (fact)
            {
                case BookingCreated created:
                    await email.SendAsync(created.CustomerEmail, $"Booking {created.BookingId} confirmed");
                    await audit.WriteAsync($"BookingCreated {created.BookingId}");
                    break;
                case BookingCancelled cancelled:
                    await email.SendAsync(cancelled.CustomerEmail,
                        $"Booking {cancelled.BookingId} cancelled; refund {cancelled.Refund.Amount} NOK");
                    await audit.WriteAsync($"BookingCancelled {cancelled.BookingId}");
                    break;
            }
        }
    }
}
