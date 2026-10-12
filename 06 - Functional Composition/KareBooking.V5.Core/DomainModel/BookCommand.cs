namespace KareBooking.V5.DomainModel;

public sealed record BookCommand(string CustomerEmail, DateTime Start, int Hours);
