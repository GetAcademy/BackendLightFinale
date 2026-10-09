namespace KareBooking.V2;

public sealed record BookCommand(string CustomerEmail, DateTimeOffset Start, int Hours);
