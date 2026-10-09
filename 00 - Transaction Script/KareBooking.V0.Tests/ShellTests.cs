using NUnit.Framework;

namespace KareBooking.V0.Tests;


public sealed class ShellTests
{
    private static readonly TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Oslo");
    private static readonly DateTimeOffset start = DateTimeOffset.Parse("2030-01-14T12:00:00+01:00");

    [Test]
    public async Task Booking_saves_charges_confirms_and_audits_then_cancellation_refunds_only_once()
    {
        var repo = new CountingRepository();
        var clock = new FixedClock(start.AddDays(-10));
        var effects = new RecordingEffects();
        var service = new BookingService(repo, clock, effects, effects, effects, zone);
        var booked = await service.BookAsync(new BookCommand("ada@example.test", start, 2));
        Assert.That(booked.IsSuccess, Is.True, booked.Error);
        Assert.That(repo.Saves, Is.EqualTo(1));
        Assert.That(effects.Calls[0], Is.EqualTo("charge:800"));
        Assert.That(effects.Calls[1], Is.EqualTo("email:ada@example.test"));
        Assert.That(effects.Calls[2], Does.StartWith("audit:BookingCreated"));

        clock.Now = start.AddDays(-3);
        var result = await service.CancelAsync(booked.Value.Id);
        Assert.That(result.IsSuccess, Is.True, result.Error);
        Assert.That(result.Value, Is.EqualTo(700m));
        Assert.That(repo.Saves, Is.EqualTo(2));
        Assert.That(effects.Calls[3], Is.EqualTo("refund:700"));
        Assert.That(effects.Calls[4], Is.EqualTo("email:ada@example.test"));
        Assert.That(effects.Calls[5], Does.StartWith("audit:BookingCancelled"));
        Assert.That((await repo.GetByIdAsync(booked.Value.Id))!.IsCancelled, Is.True);
        Assert.That((await service.CancelAsync(booked.Value.Id)).IsFailure, Is.True);
        Assert.That(effects.Calls, Has.Count.EqualTo(6));
        Assert.That(repo.Saves, Is.EqualTo(2));
    }

    [Test]
    public async Task A_quote_and_a_rejected_booking_have_no_side_effects()
    {
        var repo = new CountingRepository();
        var clock = new FixedClock(start.AddDays(-10));
        var effects = new RecordingEffects();
        var service = new BookingService(repo, clock, effects, effects, effects, zone);
        Assert.That((await service.GetQuoteAsync(new BookCommand("ada@example.test", start, 2))).IsSuccess, Is.True);
        Assert.That((await service.BookAsync(new BookCommand("ada@example.test", start.AddMinutes(37), 2))).IsFailure, Is.True);
        Assert.That((await service.CancelAsync(Guid.NewGuid())).IsFailure, Is.True);
        Assert.That(repo.Saves, Is.Zero);
        Assert.That(effects.Calls, Is.Empty);
    }

    [Test]
    public async Task Two_simultaneous_requests_cannot_book_the_same_slot()
    {
        var repo = new BlockingReadRepository();
        var clock = new FixedClock(start.AddDays(-10));
        var effects = new RecordingEffects();
        var service = new BookingService(repo, clock, effects, effects, effects, zone);
        var command = new BookCommand("ada@example.test", start, 2);
        var first = service.BookAsync(command);
        await repo.ReadObserved.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var second = service.BookAsync(command);
        repo.ReleaseReads.SetResult();
        var results = await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(results.Count(r => r.IsSuccess), Is.EqualTo(1));
        Assert.That(repo.Saves, Is.EqualTo(1));
    }

    [TestCase(168, 800)]
    [TestCase(1, 0)]
    public async Task The_shell_only_calls_refund_when_money_is_owed(int hoursBefore, decimal expected)
    {
        var repo = new CountingRepository();
        var clock = new FixedClock(start.AddDays(-10));
        var effects = new RecordingEffects();
        var service = new BookingService(repo, clock, effects, effects, effects, zone);
        var booking = await service.BookAsync(new BookCommand("ada@example.test", start, 2));
        effects.Calls.Clear();
        clock.Now = start.AddHours(-hoursBefore);

        var cancelled = await service.CancelAsync(booking.Value.Id);

        Assert.That(cancelled.IsSuccess, Is.True);
        Assert.That(cancelled.Value, Is.EqualTo(expected));
        Assert.That(effects.Calls.Count(c => c.StartsWith("refund:")), Is.EqualTo(expected > 0 ? 1 : 0));
        Assert.That(effects.Calls.Count(c => c.StartsWith("email:")), Is.EqualTo(1));
        Assert.That(effects.Calls.Count(c => c.StartsWith("audit:")), Is.EqualTo(1));
        Assert.That((await repo.GetByIdAsync(booking.Value.Id))!.IsCancelled, Is.True);
    }
}
