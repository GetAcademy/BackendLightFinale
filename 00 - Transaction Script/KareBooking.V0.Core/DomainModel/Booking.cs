namespace KareBooking.V0.Core.DomainModel;

public sealed class Booking(Guid id, string customerEmail, DateTime start, int hours, decimal price)
{
    public Guid Id { get; } = id;
    public string CustomerEmail { get; } = customerEmail;
    public DateTime Start { get; } = start;
    public int Hours { get; } = hours;
    public DateTime End => Start.AddHours(Hours);
    public decimal Price { get; } = price;
    public bool IsCancelled { get; set; }
    public Booking Copy() => new(Id, CustomerEmail, Start, Hours, Price) { IsCancelled = IsCancelled };
}
