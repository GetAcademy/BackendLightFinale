namespace KareBooking.V0;

public sealed record BookCommand(string CustomerEmail, DateTimeOffset Start, int Hours);
