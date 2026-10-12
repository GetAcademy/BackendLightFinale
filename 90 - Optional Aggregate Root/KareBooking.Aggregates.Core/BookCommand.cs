namespace KareBooking.Aggregates;

public sealed record BookCommand(string CustomerEmail, DateTime Start, int Hours);
