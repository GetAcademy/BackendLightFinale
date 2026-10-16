using KareBooking.V4.Core.ApplicationServices;
using KareBooking.V4.Core.DomainModel;
using NUnit.Framework;

namespace KareBooking.V4.Tests;

public sealed class FunctionalCoreTests
{
    [Test]
    public void Core_is_deterministic_without_any_IO_mocks_and_cancellation_keeps_the_original()
    {
        var command = new BookCommand("ada@example.test", DateTime.Parse("2030-01-14T12:00:00"), 2);
        var now = command.Start.AddDays(-14);
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var first = BookingService.Book(command, [], now, id).Value;
        var second = BookingService.Book(command, [], now, id).Value;
        Assert.That(first.Booking, Is.EqualTo(second.Booking));
        Assert.That(first.Events, Is.EqualTo(second.Events));
        var existing = new[] { first.Booking };
        Assert.That(BookingService.Book(command, existing, now, id).IsFailure, Is.True);
        var cancelled = BookingService.Cancel(first.Booking, now).Value;
        Assert.That(first.Booking.IsCancelled, Is.False);
        Assert.That(cancelled.Booking.IsCancelled, Is.True);
        Assert.That(existing[0].IsCancelled, Is.False);
        Assert.That(BookingService.Cancel(cancelled.Booking, now).IsFailure, Is.True);
    }
}
