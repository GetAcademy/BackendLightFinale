namespace KareBooking.V3;

public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(Guid id);
    Task SaveAsync(Booking booking);
}
public interface IClock { DateTimeOffset Now { get; } }
public interface IEmailSender { Task SendAsync(string recipient, string message); }
public interface IPaymentGateway
{
    Task ChargeAsync(Guid bookingId, decimal amount);
    Task RefundAsync(Guid bookingId, decimal amount);
}
public interface IAuditLog { Task WriteAsync(string message); }
