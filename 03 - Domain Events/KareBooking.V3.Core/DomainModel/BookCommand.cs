namespace KareBooking.V3.DomainModel;

public sealed record BookCommand(string CustomerEmail, DateTime Start, int Hours);
