using NUnit.Framework;

namespace KareBooking.Aggregates.Tests;

public sealed class ShellTests
{
    private static readonly DateTime start = DateTime.Parse("2030-01-14T12:00:00");

    [Test]
    public async Task A_stale_save_is_rejected_before_charge_email_or_audit()
    {
        var effects = new RecordingEffects();
        var service = new BookingService(new AlwaysConflictingRepository(),
            new FixedClock(start.AddDays(-14)), effects, effects, effects);

        var result = await service.BookAsync(new BookCommand("ada@example.test", start, 2));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.StartWith("Schedule changed"));
        Assert.That(effects.Calls, Is.Empty);
    }

    [TestCase(168, 800)]
    [TestCase(72, 700)]
    [TestCase(1, 0)]
    public async Task Successful_root_save_precedes_effects_and_cancellation_refunds_once(int hoursBefore, decimal expected)
    {
        var repository = new InMemoryScheduleRepository();
        var clock = new FixedClock(start.AddDays(-14));
        var effects = new RecordingEffects();
        var service = new BookingService(repository, clock, effects, effects, effects);
        var created = await service.BookAsync(new BookCommand("ada@example.test", start, 2));
        Assert.That(created.IsSuccess, Is.True);
        Assert.That((await repository.LoadAsync()).Version, Is.EqualTo(1));
        Assert.That(effects.Calls, Is.EqualTo(new[] { "charge:800", "email", "audit" }));
        effects.Calls.Clear();
        clock.Now = start.AddHours(-hoursBefore);

        var cancelled = await service.CancelAsync(created.Value.Id);

        Assert.That(cancelled.Value.Amount, Is.EqualTo(expected));
        Assert.That((await repository.LoadAsync()).Version, Is.EqualTo(2));
        Assert.That((await repository.LoadAsync()).Schedule.Bookings.Single().IsCancelled, Is.True);
        Assert.That(effects.Calls.Count(c => c.StartsWith("refund:")), Is.EqualTo(expected > 0 ? 1 : 0));
        var callsAfterSuccess = effects.Calls.Count;
        Assert.That((await service.CancelAsync(created.Value.Id)).IsFailure, Is.True);
        Assert.That(effects.Calls, Has.Count.EqualTo(callsAfterSuccess));
        Assert.That((await repository.LoadAsync()).Version, Is.EqualTo(2));
    }

    private sealed class AlwaysConflictingRepository() : IScheduleRepository
    {
        public Task<ScheduleSnapshot> LoadAsync() =>
            Task.FromResult(new ScheduleSnapshot(new KareSchedule(), 0));
        public Task<bool> TrySaveAsync(KareSchedule schedule, long expectedVersion) => Task.FromResult(false);
    }

    private sealed class FixedClock(DateTime now) : IClock
    {
        public DateTime Now { get; set; } = now;
    }

    private sealed class RecordingEffects : IEmailSender, IPaymentGateway, IAuditLog
    {
        public List<string> Calls { get; } = [];
        public Task SendAsync(string recipient, string message) { Calls.Add("email"); return Task.CompletedTask; }
        public Task ChargeAsync(Guid id, decimal amount) { Calls.Add($"charge:{amount}"); return Task.CompletedTask; }
        public Task RefundAsync(Guid id, decimal amount) { Calls.Add($"refund:{amount}"); return Task.CompletedTask; }
        public Task WriteAsync(string message) { Calls.Add("audit"); return Task.CompletedTask; }
    }
}
