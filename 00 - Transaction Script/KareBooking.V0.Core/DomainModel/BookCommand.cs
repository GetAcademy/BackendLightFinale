namespace KareBooking.V0.Core.DomainModel;

public sealed record BookCommand(string CustomerEmail, DateTime Start, int Hours);
