namespace KareBooking.V4;

public static class BookingCalendar
{
    // The business timezone is explicit input, including in the functional core.
    public static DateTimeOffset Deadline(DateTimeOffset start, TimeZoneInfo businessZone)
    {
        var local = TimeZoneInfo.ConvertTime(start, businessZone);
        var daysSinceMonday = ((int)local.DayOfWeek + 6) % 7;
        var friday = local.Date.AddDays(-daysSinceMonday - 3).AddHours(16);
        return new DateTimeOffset(friday, businessZone.GetUtcOffset(friday));
    }
}
