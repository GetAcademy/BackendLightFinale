using KareBooking.V6.Core.ApplicationServices;
using KareBooking.Composition;
using KareBooking.Composition.Console;
using KareBooking.V6.Core.DomainModel;

Func<int, int> addOne = x => x + 1;
Func<int, int> doubleIt = x => x * 2;
Action<string> show = Console.WriteLine;
show($"Functions as values: {FunctionExamples.Compose(addOne, doubleIt)(3)}");
var addTwo = FunctionExamples.Twice(addOne); // Function passed in; new function returned.
show($"Higher-order function Twice: {addTwo(3)}");

var command = new BookCommand("ada@example.test", DateTime.Parse("2030-01-14T12:00:00"), 2);
var now = DateTime.Parse("2030-01-11T16:00:00");
var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
var result = BookingService.Book(command, [], now, id);
show(result.IsSuccess ? $"Pipeline: {result.Value.Booking.Price.Amount} NOK" : result.Error);
var rejected = BookingService.Book(command, [result.Value.Booking], now, id);
show($"Failure track (occupied slot): {rejected.Error}");
