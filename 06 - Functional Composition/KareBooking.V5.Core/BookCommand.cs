namespace KareBooking.V5;

public sealed record BookCommand(string CustomerEmail, DateTimeOffset Start, int Hours);
