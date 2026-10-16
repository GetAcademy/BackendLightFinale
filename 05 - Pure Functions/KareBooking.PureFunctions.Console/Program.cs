using KareBooking.PureFunctions;
using KareBooking.PureFunctions.Console;

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

Console.WriteLine($"Same input, same output: {PureExamples.Price(2)} == {PureExamples.Price(2)}");
var fact = new BookingCreated(booking.Id, "ada@example.test");
Console.WriteLine($"Fact: booking {fact.BookingId} was created. This is not a SaveBooking instruction.");
var effects = EffectExamples.Plan(booking, fact.CustomerEmail); // Data only.
EffectExamples.Interpret(effects); // Only here do we perform (simulated) effects.
