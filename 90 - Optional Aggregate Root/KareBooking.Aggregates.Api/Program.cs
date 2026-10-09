using KareBooking.Aggregates;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(TimeZoneInfo.FindSystemTimeZoneById("Europe/Oslo"));
builder.Services.AddSingleton<IScheduleRepository, InMemoryScheduleRepository>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IEmailSender, ConsoleEmailSender>();
builder.Services.AddSingleton<IPaymentGateway, FakePaymentGateway>();
builder.Services.AddSingleton<IAuditLog, ConsoleAuditLog>();

builder.Services.AddSingleton<BookingService>();
var app = builder.Build();

app.MapGet("/quote", async (DateTimeOffset start, int hours, string customerEmail, BookingService service) =>
{
    var result = await service.GetQuoteAsync(new BookCommand(customerEmail, start, hours));
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
});
app.MapPost("/bookings", async (BookCommand command, BookingService service) =>
{
    var result = await service.BookAsync(command);
    return result.IsSuccess ? Results.Created($"/bookings/{result.Value.Id}", result.Value)
        : Results.BadRequest(new { error = result.Error });
});
app.MapPost("/bookings/{id:guid}/cancel", async (Guid id, BookingService service) =>
{
    var result = await service.CancelAsync(id);
    if (result.IsSuccess) return Results.Ok(new { refund = result.Value });
    return result.Error == "Booking not found." ? Results.NotFound(new { error = result.Error })
        : Results.BadRequest(new { error = result.Error });
});
app.MapGet("/bookings/{id:guid}", async (Guid id, IScheduleRepository repository) =>
{
    var booking = (await repository.LoadAsync()).Schedule.Bookings.FirstOrDefault(b => b.Id == id);
    return booking is null ? Results.NotFound() : Results.Ok(booking);
});
// In-memory teaching state; no personal information is persisted outside this process.
app.MapGet("/bookings", async (IScheduleRepository repository) => (await repository.LoadAsync()).Schedule.Bookings);
app.Run();
