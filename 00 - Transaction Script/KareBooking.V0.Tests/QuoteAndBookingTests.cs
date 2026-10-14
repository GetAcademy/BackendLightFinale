using KareBooking.V0.Api.Infrastructure;
using KareBooking.V0.Core.ApplicationServices;
using KareBooking.V0.Core.DomainModel;
using NUnit.Framework;

namespace KareBooking.V0.Tests;

public sealed class QuoteAndBookingTests
{
    [TestCase("2030-01-14T12:00:00", 1, "2030-01-11T16:00:00")]
    [TestCase("2030-01-14T12:00:00", 2, "2030-01-11T16:00:00")]
    [TestCase("2030-01-14T12:00:00", 3, "2030-01-11T16:00:00")]
    [TestCase("2030-01-14T12:00:00", 4, "2030-01-11T16:00:00")]
    [TestCase("2030-01-19T12:00:00", 1, "2030-01-01T12:00:00")]
    [TestCase("2030-01-20T12:00:00", 1, "2030-01-01T12:00:00")]
    [TestCase("2030-01-14T11:00:00", 1, "2030-01-01T12:00:00")]
    [TestCase("2030-01-14T15:00:00", 2, "2030-01-01T12:00:00")]
    [TestCase("2030-01-14T12:37:00", 1, "2030-01-01T12:00:00")]
    [TestCase("2030-01-14T12:00:00.0000001", 1, "2030-01-01T12:00:00")]
    [TestCase("2030-01-14T12:00:00", 0, "2030-01-01T12:00:00")]
    [TestCase("2030-01-14T12:00:00", -1, "2030-01-01T12:00:00")]
    [TestCase("2030-01-14T12:00:00", 5, "2030-01-01T12:00:00")]
    [TestCase("2030-01-14T12:00:00", 1, "2030-01-11T16:00:00.0000001")]
    [TestCase("2030-01-18T12:00:00", 1, "2030-01-14T12:00:00")]
    [TestCase("2030-04-01T12:00:00", 1, "2030-03-29T16:00:00")]
    public async Task Quote_and_booking_have_consistent_validation_and_prices(string startText, int hours, string nowText)
    {
        var repository = new InMemoryBookingRepository();
        var effects = new RecordingEffects();
        var service = new BookingService(repository, new FixedClock(DateTime.Parse(nowText)),
            effects, effects, effects);
        var command = new BookCommand("ada@example.test", DateTime.Parse(startText), hours);

        var quote = await service.GetQuoteAsync(command);
        var booked = await service.BookAsync(command);

        Assert.That(booked.IsSuccess, Is.EqualTo(quote.IsSuccess));
        if (quote.IsSuccess) Assert.That(booked.Value.Price, Is.EqualTo(quote.Value));
        else Assert.That(booked.Error, Is.EqualTo(quote.Error));
    }

    [Test]
    public async Task Quote_and_booking_reject_overlap_and_accept_the_adjacent_slot()
    {
        var start = DateTime.Parse("2030-01-14T12:00:00");
        var repository = new InMemoryBookingRepository();
        var effects = new RecordingEffects();
        var service = new BookingService(repository, new FixedClock(start.AddDays(-14)),
            effects, effects, effects);
        var command = new BookCommand("ada@example.test", start, 2);
        Assert.That((await service.BookAsync(command)).IsSuccess, Is.True);
        Assert.That((await service.GetQuoteAsync(command)).IsFailure, Is.True);
        Assert.That((await service.BookAsync(command)).IsFailure, Is.True);
        var adjacent = command with { Start = start.AddHours(2) };
        Assert.That((await service.GetQuoteAsync(adjacent)).IsSuccess, Is.True);
        Assert.That((await service.BookAsync(adjacent)).IsSuccess, Is.True);
    }
}
