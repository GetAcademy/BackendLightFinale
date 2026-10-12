namespace KareBooking.V5.DomainModel;

public sealed record TimeSlot
{
    public DateTime Start { get; }
    public int Hours { get; }
    public DateTime End => Start.AddHours(Hours);
    private TimeSlot(DateTime start, int hours) { Start = start; Hours = hours; }

    public static Result<TimeSlot> Create(DateTime start, int hours)
    {
        var local = start;
        if (local.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return Result<TimeSlot>.Failure("Kare works Monday to Friday.");
        if (local.Minute != 0 || local.Second != 0 || local.Ticks % TimeSpan.TicksPerSecond != 0)
            return Result<TimeSlot>.Failure("Start must be a whole clock hour.");
        if (hours is < 1 or > 4)
            return Result<TimeSlot>.Failure("Duration must be 1 to 4 whole hours.");
        if (local.Hour < 12 || local.Hour + hours > 16)
            return Result<TimeSlot>.Failure("The booking must fit within 12:00–16:00.");
        return Result<TimeSlot>.Success(new TimeSlot(local, hours));
    }

    // Half-open intervals: a booking ending at 13:00 permits one starting at 13:00.
    public bool Overlaps(TimeSlot other) => Start < other.End && End > other.Start;
}
