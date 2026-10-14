namespace KareBooking.Aggregates.Core;

public sealed record BookCommand(string CustomerEmail, DateTime Start, int Hours);
