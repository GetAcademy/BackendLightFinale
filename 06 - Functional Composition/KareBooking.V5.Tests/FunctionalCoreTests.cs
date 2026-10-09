using NUnit.Framework;

namespace KareBooking.V5.Tests;


public sealed class FunctionalCoreTests
{
    [Test]
    public void Core_is_deterministic_without_any_IO_mocks_and_cancellation_keeps_the_original()
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Oslo");
        var command = new BookCommand("ada@example.test", DateTimeOffset.Parse("2030-01-14T12:00:00+01:00"), 2);
        var now = command.Start.AddDays(-14);
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var first = BookingWorkflow.Book(command, [], now, id, zone).Value;
        var second = BookingWorkflow.Book(command, [], now, id, zone).Value;
        Assert.That(first.Booking, Is.EqualTo(second.Booking));
        Assert.That(first.Events, Is.EqualTo(second.Events));
        var existing = new[] { first.Booking };
        Assert.That(BookingWorkflow.Book(command, existing, now, id, zone).IsFailure, Is.True);
        var cancelled = BookingWorkflow.Cancel(first.Booking, now).Value;
        Assert.That(first.Booking.IsCancelled, Is.False);
        Assert.That(cancelled.Booking.IsCancelled, Is.True);
        Assert.That(existing[0].IsCancelled, Is.False);
        Assert.That(BookingWorkflow.Cancel(cancelled.Booking, now).IsFailure, Is.True);
    }
}
