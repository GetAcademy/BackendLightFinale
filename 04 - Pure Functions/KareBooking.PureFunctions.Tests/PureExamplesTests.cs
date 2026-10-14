using KareBooking.PureFunctions.Console;
using NUnit.Framework;

namespace KareBooking.PureFunctions.Tests;

public sealed class PureExamplesTests
{
    [Test]
    public void Explicit_inputs_give_the_same_booking_each_time()
    {
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var start = DateTime.Parse("2030-01-14T12:00:00");
        Assert.That(PureExamples.Create(id, start, 800), Is.EqualTo(PureExamples.Create(id, start, 800)));
        Assert.That(PureExamples.Discount(2), Is.EqualTo(200));
        Assert.That(PureExamples.Discount(2), Is.EqualTo(200));
    }

    [Test]
    public void Cancellation_with_record_does_not_change_the_original()
    {
        var original = PureExamples.Create(Guid.NewGuid(), DateTime.Parse("2030-01-14T12:00:00"), 800);
        var cancelled = PureExamples.Cancel(original);
        Assert.That(original.IsCancelled, Is.False);
        Assert.That(cancelled.IsCancelled, Is.True);
        Assert.That(cancelled.Id, Is.EqualTo(original.Id));
    }

    [Test]
    public void The_clock_is_plain_input_and_the_start_boundary_is_explicit()
    {
        var start = DateTime.Parse("2030-01-14T12:00:00");
        var booking = PureExamples.Create(Guid.NewGuid(), start, 800);
        Assert.That(PureExamples.CanCancel(booking, start.AddTicks(-1)), Is.True);
        Assert.That(PureExamples.CanCancel(booking, start), Is.False);
    }

    [TestCase(1, 500)] [TestCase(2, 800)] [TestCase(3, 1100)] [TestCase(4, 1400)]
    public void Pure_price_depends_only_on_hours(int hours, decimal expected) => Assert.That(PureExamples.Price(hours), Is.EqualTo(expected));
}
