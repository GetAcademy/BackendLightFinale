namespace KareBooking.V1.DomainModel;

public sealed record BookCommand(string CustomerEmail, DateTime Start, int Hours);
