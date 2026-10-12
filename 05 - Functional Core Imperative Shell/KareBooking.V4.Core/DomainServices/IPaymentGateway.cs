namespace KareBooking.V4.DomainServices;

public interface IPaymentGateway
{
    Task ChargeAsync(Guid bookingId, decimal amount);
    Task RefundAsync(Guid bookingId, decimal amount);
}
