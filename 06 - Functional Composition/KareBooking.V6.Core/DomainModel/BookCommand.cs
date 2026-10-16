namespace KareBooking.V6.Core.DomainModel;

public sealed record BookCommand(string CustomerEmail, DateTime Start, int Hours);
