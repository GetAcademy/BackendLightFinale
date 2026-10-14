namespace KareBooking.PreludeA.Api.Infrastructure;

public sealed class SystemClock { public DateTime Now => DateTime.Now; }

public sealed class ConsoleEmailSender
{
    public Task SendAsync(string recipient, string message)
    { Console.WriteLine($"EMAIL to {recipient}: {message}"); return Task.CompletedTask; }
}

// Deliberately always succeeds: no real money moves and no payment credentials are needed.
public sealed class FakePaymentGateway
{
    public Task ChargeAsync(Guid bookingId, decimal amount)
    { Console.WriteLine($"FAKE CHARGE {bookingId}: {amount} NOK"); return Task.CompletedTask; }
    public Task RefundAsync(Guid bookingId, decimal amount)
    { Console.WriteLine($"FAKE REFUND {bookingId}: {amount} NOK"); return Task.CompletedTask; }
}

public sealed class ConsoleAuditLog
{
    public Task WriteAsync(string message) { Console.WriteLine($"AUDIT {message}"); return Task.CompletedTask; }
}

