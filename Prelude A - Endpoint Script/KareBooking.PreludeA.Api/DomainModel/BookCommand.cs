namespace KareBooking.PreludeA.Api.DomainModel;

public sealed record BookCommand(string CustomerEmail, DateTime Start, int Hours);

