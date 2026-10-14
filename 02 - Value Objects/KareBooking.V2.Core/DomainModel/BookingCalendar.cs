namespace KareBooking.V2.Core.DomainModel;

public static class BookingCalendar
{
    // Booking closes on Friday at 16:00 in the week before the appointment.
    public static DateTime Deadline(DateTime start)
    {
        var daysSinceMonday = ((int)start.DayOfWeek + 6) % 7;
        var friday = start.Date.AddDays(-daysSinceMonday - 3).AddHours(16);
        return friday;
    }
}
