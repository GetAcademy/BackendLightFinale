namespace KareBooking.V1;

public sealed record BookCommand(string CustomerEmail, DateTimeOffset Start, int Hours);
