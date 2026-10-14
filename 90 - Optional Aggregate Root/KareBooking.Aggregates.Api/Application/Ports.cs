using KareBooking.Aggregates.Core;

namespace KareBooking.Aggregates.Api.Application;

public sealed record ScheduleSnapshot(KareSchedule Schedule, long Version);

// Repository per consistency boundary, not per table or child entity.
public interface IScheduleRepository
{
    Task<ScheduleSnapshot> LoadAsync();
    Task<bool> TrySaveAsync(KareSchedule schedule, long expectedVersion);
}
public interface IClock { DateTime Now { get; } }
public interface IEmailSender { Task SendAsync(string recipient, string message); }
public interface IPaymentGateway
{
    Task ChargeAsync(Guid bookingId, decimal amount);
    Task RefundAsync(Guid bookingId, decimal amount);
}
public interface IAuditLog { Task WriteAsync(string message); }
