namespace KareBooking.V4.DomainModel;

public sealed record BookCommand(string CustomerEmail, DateTime Start, int Hours);
