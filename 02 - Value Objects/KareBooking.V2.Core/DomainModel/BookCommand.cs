namespace KareBooking.V2.DomainModel;

public sealed record BookCommand(string CustomerEmail, DateTime Start, int Hours);
