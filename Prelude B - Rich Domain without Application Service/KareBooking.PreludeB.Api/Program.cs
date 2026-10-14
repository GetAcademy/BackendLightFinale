using KareBooking.PreludeB.Api.DomainModel;
using KareBooking.PreludeB.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<InMemoryBookingRepository>();
builder.Services.AddSingleton<SystemClock>();
builder.Services.AddSingleton<FakePaymentGateway>();
builder.Services.AddSingleton<ConsoleEmailSender>();
builder.Services.AddSingleton<ConsoleAuditLog>();
var app = builder.Build();

app.MapGet("/quote", async (DateTime start, int hours, string customerEmail,
    InMemoryBookingRepository repository, SystemClock clock) =>
{
    var existing = await repository.GetAllAsync();
    var result = new KareSchedule(existing).GetQuote(new BookCommand(customerEmail, start, hours), clock.Now);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
});

app.MapPost("/bookings", async (BookCommand command,
    InMemoryBookingRepository repository, SystemClock clock, FakePaymentGateway payment,
    ConsoleEmailSender email, ConsoleAuditLog audit) =>
{
    var existing = await repository.GetAllAsync();
    var now = clock.Now;
    var id = Guid.NewGuid();
    var decision = new KareSchedule(existing).Book(command, now, id);
    if (decision.IsFailure) return Results.BadRequest(new { error = decision.Error });

    var booking = decision.Value;
    await repository.SaveAsync(booking);
    await payment.ChargeAsync(booking.Id, booking.Price);
    await email.SendAsync(booking.CustomerEmail, $"Booking {booking.Id} confirmed");
    await audit.WriteAsync($"BookingCreated {booking.Id}");
    return Results.Created($"/bookings/{booking.Id}", booking);
});

app.MapPost("/bookings/{id:guid}/cancel", async (Guid id,
    InMemoryBookingRepository repository, SystemClock clock, FakePaymentGateway payment,
    ConsoleEmailSender email, ConsoleAuditLog audit) =>
{
    var booking = await repository.GetByIdAsync(id);
    if (booking is null) return Results.NotFound(new { error = "Booking not found." });
    var decision = booking.Cancel(clock.Now);
    if (decision.IsFailure) return Results.BadRequest(new { error = decision.Error });

    var refund = decision.Value;
    await repository.SaveAsync(booking);
    if (refund > 0) await payment.RefundAsync(id, refund);
    await email.SendAsync(booking.CustomerEmail, $"Booking {booking.Id} cancelled; refund {refund} NOK");
    await audit.WriteAsync($"BookingCancelled {booking.Id}");
    return Results.Ok(new { refund });
});

app.MapGet("/bookings/{id:guid}", async (Guid id, InMemoryBookingRepository repository) =>
{
    var booking = await repository.GetByIdAsync(id);
    return booking is null ? Results.NotFound() : Results.Ok(booking);
});
app.MapGet("/bookings", (InMemoryBookingRepository repository) => repository.GetAllAsync());
app.Run();
