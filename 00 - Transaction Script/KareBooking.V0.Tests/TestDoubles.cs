using KareBooking.V0.DomainModel;
using KareBooking.V0.DomainServices;
using KareBooking.V0.Infrastructure;

namespace KareBooking.V0.Tests;

public sealed class FixedClock(DateTime now) : IClock
{
    public DateTime Now { get; set; } = now;
}

public sealed class RecordingEffects : IEmailSender, IPaymentGateway, IAuditLog
{
    public List<string> Calls { get; } = [];
    public Task SendAsync(string recipient, string message)
    { Calls.Add($"email:{recipient}"); return Task.CompletedTask; }
    public Task ChargeAsync(Guid id, decimal amount)
    { Calls.Add($"charge:{amount}"); return Task.CompletedTask; }
    public Task RefundAsync(Guid id, decimal amount)
    { Calls.Add($"refund:{amount}"); return Task.CompletedTask; }
    public Task WriteAsync(string message) { Calls.Add($"audit:{message}"); return Task.CompletedTask; }
}

public sealed class CountingRepository : IBookingRepository
{
    private readonly InMemoryBookingRepository inner = new();
    public int Saves { get; private set; }
    public Task<IReadOnlyList<Booking>> GetAllAsync() => inner.GetAllAsync();
    public Task<Booking?> GetByIdAsync(Guid id) => inner.GetByIdAsync(id);
    public Task SaveAsync(Booking booking) { Saves++; return inner.SaveAsync(booking); }
}
