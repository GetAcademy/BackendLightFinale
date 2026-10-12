using KareBooking.PureFunctions;

var now = DateTime.Now; // I/O at the edge of the demonstration.
var id = Guid.NewGuid();
var choice = Random.Shared.Next(0, 3);
var booking = PureExamples.Create(id, now.AddDays(10), PureExamples.Price(2));
Console.WriteLine($"Hidden clock: {ImpureExamples.CanCancel(booking)}");
Console.WriteLine($"Explicit clock: {PureExamples.CanCancel(booking, now)}");
Console.WriteLine($"Hidden random: {ImpureExamples.PickDiscount()}");
Console.WriteLine($"Explicit random input: {PureExamples.Discount(choice)}");
var mutable = new MutableBooking();
ImpureExamples.Cancel(mutable);
var cancelled = PureExamples.Cancel(booking);
Console.WriteLine($"Mutation: {mutable.IsCancelled}; record original: {booking.IsCancelled}; copy: {cancelled.IsCancelled}");
Console.WriteLine($"I/O comparison: QuoteFromFile reads a file; Price(2) only calculates: {PureExamples.Price(2)}");
