namespace KareBooking.V6.Api.ImperativeShell.Ports;

public interface IPaymentGateway
{
    Task ChargeAsync(Guid bookingId, decimal amount);
    Task RefundAsync(Guid bookingId, decimal amount);
}
