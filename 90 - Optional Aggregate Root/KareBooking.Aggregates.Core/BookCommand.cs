namespace KareBooking.Aggregates;

public sealed record BookCommand(string CustomerEmail, DateTimeOffset Start, int Hours);
