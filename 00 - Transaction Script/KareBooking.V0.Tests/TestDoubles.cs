namespace KareBooking.V0.Tests;

public sealed class FixedClock(DateTimeOffset now) : IClock
{
    public DateTimeOffset Now { get; set; } = now;
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

// Freeze the first read before save, so the concurrency test really interleaves requests.
public sealed class BlockingReadRepository : IBookingRepository
{
    private readonly InMemoryBookingRepository inner = new();
    public TaskCompletionSource ReadObserved { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource ReleaseReads { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public int Saves { get; private set; }

    public async Task<IReadOnlyList<Booking>> GetAllAsync()
    {
        var snapshot = await inner.GetAllAsync();
        ReadObserved.TrySetResult();
        await ReleaseReads.Task;
        return snapshot;
    }

    public Task<Booking?> GetByIdAsync(Guid id) => inner.GetByIdAsync(id);
    public Task SaveAsync(Booking booking) { Saves++; return inner.SaveAsync(booking); }
}
