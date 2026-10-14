namespace KareBooking.PreludeB.Api.DomainModel;

public sealed record BookCommand(string CustomerEmail, DateTime Start, int Hours);

