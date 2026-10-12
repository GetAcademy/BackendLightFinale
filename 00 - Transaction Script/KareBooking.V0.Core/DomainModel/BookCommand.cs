namespace KareBooking.V0.DomainModel;

public sealed record BookCommand(string CustomerEmail, DateTime Start, int Hours);
