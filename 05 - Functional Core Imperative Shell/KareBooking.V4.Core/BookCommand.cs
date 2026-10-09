namespace KareBooking.V4;

public sealed record BookCommand(string CustomerEmail, DateTimeOffset Start, int Hours);
