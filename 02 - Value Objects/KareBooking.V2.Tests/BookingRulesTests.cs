using KareBooking.V2.DomainModel;

using NUnit.Framework;

namespace KareBooking.V2.Tests;

public sealed class BookingRulesTests
{
    private static readonly DateTime monday = DateTime.Parse("2030-01-14T12:00:00");
    private static readonly DateTime early = monday.AddDays(-14);

    private static async Task<Result<Money>> Quote(BookCommand command, DateTime now,
        params Booking[] existing)
    {
        return await Task.FromResult(new KareSchedule(existing).GetQuote(command, now));
    }
    private static async Task<Booking> Create(DateTime start, int hours = 2)
    {
        var result = new KareSchedule([]).Book(new BookCommand("ada@example.test", start, hours), start.AddDays(-14), Guid.NewGuid());
        return await Task.FromResult(result.Value);
    }
    private static async Task<Result<Money>> Cancel(Booking booking, DateTime now)
    {
        return await Task.FromResult(booking.Cancel(now));
    }

    [TestCase("2030-01-19T12:00:00", TestName = "Booking_on_saturday_is_rejected")]
    [TestCase("2030-01-20T12:00:00", TestName = "Booking_on_sunday_is_rejected")]
    [TestCase("2030-01-14T11:00:00", TestName = "Booking_before_noon_is_rejected")]
    [TestCase("2030-01-14T16:00:00", TestName = "Booking_starting_at_closing_is_rejected")]
    [TestCase("2030-01-14T12:37:00", TestName = "Start_must_be_a_whole_clock_hour")]
    [TestCase("2030-01-14T12:00:01", TestName = "Seconds_are_not_a_whole_clock_hour")]
    [TestCase("2030-01-14T12:00:00.0000001", TestName = "Fractional_seconds_are_rejected")]
    public async Task Invalid_start_is_rejected(string start)
    {
        var result = await Quote(new BookCommand("ada@example.test", DateTime.Parse(start), 1), early);
        Assert.That(result.IsFailure, Is.True);
    }

    [TestCase(0)] [TestCase(-1)] [TestCase(5)] [TestCase(int.MaxValue)]
    public async Task Duration_must_be_positive_and_fit_the_workday(int hours) =>
        Assert.That((await Quote(new BookCommand("ada@example.test", monday, hours), early)).IsFailure, Is.True);

    [Test]
    public async Task Booking_ending_after_16_is_rejected() =>
        Assert.That((await Quote(new BookCommand("ada@example.test", monday.AddHours(3), 2), early)).IsFailure, Is.True);

    [TestCase(1, 500)] [TestCase(2, 800, TestName = "Second_hour_costs_300")]
    [TestCase(3, 1100)] [TestCase(4, 1400)]
    public async Task First_hour_costs_500_and_each_extra_hour_costs_300(int hours, decimal price)
    {
        var quote = await Quote(new BookCommand("ada@example.test", monday, hours), early);
        Assert.That(quote.IsSuccess, Is.True, quote.Error);
        Assert.That(quote.Value.Amount, Is.EqualTo(price));
    }

    [TestCase(0)] [TestCase(1)]
    public async Task Overlapping_bookings_are_rejected(int startHours)
    {
        var existing = await Create(monday);
        Assert.That((await Quote(new BookCommand("ada@example.test", monday.AddHours(startHours), 1), early, existing)).IsFailure, Is.True);
    }

    [Test]
    public async Task A_booking_containing_an_existing_booking_is_rejected()
    {
        var existing = await Create(monday.AddHours(1), 1);
        Assert.That((await Quote(new BookCommand("ada@example.test", monday, 4), early, existing)).IsFailure, Is.True);
    }

    [Test]
    public async Task Adjacent_slots_and_other_days_do_not_overlap()
    {
        var existing = await Create(monday.AddHours(1), 1);
        Assert.That((await Quote(new BookCommand("ada@example.test", monday, 1), early, existing)).IsSuccess, Is.True);
        Assert.That((await Quote(new BookCommand("ada@example.test", monday.AddHours(2), 2), early, existing)).IsSuccess, Is.True);
        Assert.That((await Quote(new BookCommand("ada@example.test", monday.AddDays(1), 2), early, existing)).IsSuccess, Is.True);
    }

    [Test]
    public async Task Cancelled_bookings_release_the_slot()
    {
        var existing = await Create(monday);
        await Cancel(existing, early);
        Assert.That((await Quote(new BookCommand("ada@example.test", monday, 2), early, existing)).IsSuccess, Is.True);
    }

    [TestCase("2030-01-14T12:00:00", "2030-01-11T16:00:00")]
    [TestCase("2030-01-18T12:00:00", "2030-01-11T16:00:00")]
    [TestCase("2027-01-01T12:00:00", "2026-12-25T16:00:00")]
    [TestCase("2030-04-01T12:00:00", "2030-03-29T16:00:00")]
    [TestCase("2030-10-28T12:00:00", "2030-10-25T16:00:00")]
    public async Task Deadline_is_friday_in_the_previous_week_including_year_and_month_boundaries(string startText, string deadlineText)
    {
        var start = DateTime.Parse(startText);
        var deadline = DateTime.Parse(deadlineText);
        Assert.That(BookingCalendar.Deadline(start), Is.EqualTo(deadline));
        var command = new BookCommand("ada@example.test", start, 1);
        Assert.That((await Quote(command, deadline.AddTicks(-1))).IsSuccess, Is.True);
        Assert.That((await Quote(command, deadline)).IsSuccess, Is.True, "Exactly 16:00 is allowed.");
        Assert.That((await Quote(command, deadline.AddTicks(1))).IsFailure, Is.True);
    }

    [Test]
    public async Task Booking_on_monday_of_the_same_week_is_too_late() =>
        Assert.That((await Quote(new BookCommand("ada@example.test", monday.AddDays(4), 1), monday)).IsFailure, Is.True);

    [Test]
    public async Task Local_business_time_is_used_directly() =>
        Assert.That((await Quote(new BookCommand("ada@example.test", monday, 1), early)).IsSuccess, Is.True);

    [TestCase(240, 800)] [TestCase(168, 800, TestName = "Exactly_seven_days_before_gives_full_refund")]
    [TestCase(167.999, 700)] [TestCase(72, 700, TestName = "Cancellation_three_days_before_keeps_100_fee")]
    [TestCase(48, 700, TestName = "Exactly_48_hours_before_keeps_100_fee")]
    [TestCase(47.999, 0)] [TestCase(1, 0)]
    public async Task Refund_boundaries_are_measured_in_elapsed_hours(double hoursBefore, decimal expected)
    {
        var booking = await Create(monday);
        var refund = await Cancel(booking, monday.AddHours(-hoursBefore));
        Assert.That(refund.IsSuccess, Is.True, refund.Error);
        Assert.That(refund.Value.Amount, Is.EqualTo(expected));
    }

    [Test]
    public async Task Seven_local_days_before_gives_full_refund()
    {
        var start = DateTime.Parse("2030-04-01T12:00:00");
        var booking = await Create(start);
        var refund = await Cancel(booking, DateTime.Parse("2030-03-25T12:00:00"));
        Assert.That(refund.Value.Amount, Is.EqualTo(800m));
    }

    [TestCase(0)] [TestCase(-1)]
    public async Task A_started_booking_cannot_be_cancelled(int hoursBefore)
    {
        var booking = await Create(monday);
        Assert.That((await Cancel(booking, monday.AddHours(-hoursBefore))).IsFailure, Is.True);
    }

    [Test]
    public async Task Customer_email_is_required() =>
        Assert.That((await Quote(new BookCommand("", monday, 1), early)).IsFailure, Is.True);
}
