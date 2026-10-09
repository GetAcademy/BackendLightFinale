namespace KareBooking.PureFunctions;

public sealed class MutableBooking { public bool IsCancelled { get; set; } }
public sealed record BookingValue(Guid Id, DateTimeOffset Start, decimal Price, bool IsCancelled = false);

public static class ImpureExamples
{
    public static bool CanCancel(BookingValue booking) => booking.Start > DateTimeOffset.Now;
    public static int PickDiscount() => Random.Shared.Next(0, 3) * 100;
    public static BookingValue Create(DateTimeOffset start, decimal price) => new(Guid.NewGuid(), start, price);
    public static void Cancel(MutableBooking booking) => booking.IsCancelled = true;
    public static decimal QuoteFromFile(string path) => PureExamples.Price(int.Parse(File.ReadAllText(path)));
}

public static class PureExamples
{
    public static bool CanCancel(BookingValue booking, DateTimeOffset now) => booking.Start > now;
    public static int Discount(int generatedChoice) => generatedChoice * 100;
    public static BookingValue Create(Guid generatedId, DateTimeOffset start, decimal price) => new(generatedId, start, price);
    public static BookingValue Cancel(BookingValue booking) => booking with { IsCancelled = true };
    public static decimal Price(int hours) => 500m + (hours - 1) * 300m;
}
