namespace KareBooking.V3;

public sealed record BookCommand(string CustomerEmail, DateTimeOffset Start, int Hours);
