using KareBooking.Composition;
using KareBooking.V5;

Func<int, int> addOne = x => x + 1;
Func<int, int> doubleIt = x => x * 2;
Action<string> show = Console.WriteLine;
show($"Functions as values: {FunctionExamples.Compose(addOne, doubleIt)(3)}");
show($"Higher-order function Twice: {FunctionExamples.Twice(addOne)(3)}");
show($"Currying: {FunctionExamples.AddCurried(2)(3)}");
var addTwo = FunctionExamples.AddCurried(2);
show($"Partial application: {addTwo(3)}");
var price = FunctionExamples.PriceWithFirstHour(500m);
show($"Partially applied pricing: {price(2)} NOK");

var command = new BookCommand("ada@example.test", DateTimeOffset.Parse("2030-01-14T12:00:00+01:00"), 2);
var now = DateTimeOffset.Parse("2030-01-11T16:00:00+01:00");
var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
var result = BookingWorkflow.Book(command, [], now, id, TimeZoneInfo.FindSystemTimeZoneById("Europe/Oslo"));
show(result.IsSuccess ? $"Pipeline: {result.Value.Booking.Price.Amount} NOK" : result.Error);
