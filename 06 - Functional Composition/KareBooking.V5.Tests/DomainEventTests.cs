using KareBooking.V5.DomainModel;

using NUnit.Framework;

namespace KareBooking.V5.Tests;

public sealed class DomainEventTests
{
    [Test]
    public void Domain_operations_produce_business_facts_without_sending_email()
    {
        var command = new BookCommand("ada@example.test", DateTime.Parse("2030-01-14T12:00:00"), 2);
        var now = command.Start.AddDays(-14);
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var created = BookingWorkflow.Book(command, [], now, id);
        Assert.That(created.Value.Events, Has.Count.EqualTo(1));
        var fact = (BookingCreated)created.Value.Events.Single();
        Assert.That(fact.BookingId, Is.EqualTo(id));
        Assert.That(fact.CustomerEmail, Is.EqualTo(command.CustomerEmail));
        Assert.That(fact.Price.Amount, Is.EqualTo(800m));
        var cancelled = BookingWorkflow.Cancel(created.Value.Booking, now);
        Assert.That(cancelled.Value.Events.Single(), Is.TypeOf<BookingCancelled>());
        Assert.That(((BookingCancelled)cancelled.Value.Events.Single()).Refund.Amount, Is.EqualTo(800m));
    }
}
