using KareBooking.V2.DomainModel;

using NUnit.Framework;

namespace KareBooking.V2.Tests;

public sealed class ValueObjectTests
{
    private static readonly DateTime start = DateTime.Parse("2030-01-14T12:00:00");

    [TestCase(0)] [TestCase(-2)] [TestCase(5)]
    public void Invalid_TimeSlot_cannot_be_created(int hours) =>
        Assert.That(TimeSlot.Create(start, hours).IsFailure, Is.True);

    [Test]
    public void Minute_start_and_late_end_cannot_be_represented()
    {
        Assert.That(TimeSlot.Create(start.AddMinutes(37), 1).IsFailure, Is.True);
        Assert.That(TimeSlot.Create(start.AddHours(3), 2).IsFailure, Is.True);
    }

    [TestCase(-1)] [TestCase(0.001)]
    public void Money_rejects_negative_amounts_and_fractional_ore(decimal amount) =>
        Assert.That(Money.Create(amount).IsFailure, Is.True);

    [Test]
    public void Values_compare_by_content()
    {
        Assert.That(TimeSlot.Create(start, 2).Value, Is.EqualTo(TimeSlot.Create(new DateTime(2030, 1, 14, 12, 0, 0), 2).Value));
        Assert.That(Money.Create(800).Value, Is.EqualTo(Money.ForHours(2)));
        Assert.That(Money.ForHours(2).Currency, Is.EqualTo("NOK"));
        Assert.That(TimeSlot.Create(start, 2).Value.Hours, Is.EqualTo(2));
    }
}
