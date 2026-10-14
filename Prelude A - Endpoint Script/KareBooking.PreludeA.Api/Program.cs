using KareBooking.PreludeA.Api.DomainModel;
using KareBooking.PreludeA.Api.Infrastructure;

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
    var command = new BookCommand(customerEmail, start, hours);
    var localTime = command.Start;
    if (string.IsNullOrWhiteSpace(command.CustomerEmail) || !command.CustomerEmail.Contains('@'))
        return Results.BadRequest(new { error = "A customer email is required." });
    if (localTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        return Results.BadRequest(new { error = "Kare works Monday to Friday." });
    if (localTime.Minute != 0 || localTime.Second != 0 || localTime.Ticks % TimeSpan.TicksPerSecond != 0)
        return Results.BadRequest(new { error = "Start must be a whole clock hour." });
    if (command.Hours is < 1 or > 4)
        return Results.BadRequest(new { error = "Duration must be 1 to 4 whole hours." });
    if (localTime.Hour < 12 || localTime.Hour + command.Hours > 16)
        return Results.BadRequest(new { error = "The booking must fit within 12:00–16:00." });

    var daysSinceMonday = ((int)localTime.DayOfWeek + 6) % 7;
    var deadline = localTime.Date.AddDays(-daysSinceMonday - 3).AddHours(16);
    if (clock.Now > deadline)
        return Results.BadRequest(new { error = "Book no later than Friday 16:00 in the previous week." });

    var existing = await repository.GetAllAsync();
    if (existing.Any(booking =>
    {
        if (booking.IsCancelled) return false;
        var end = localTime.AddHours(command.Hours);
        var endsBeforeOrAtStart = end <= booking.Start;
        var startsAfterOrAtEnd = localTime >= booking.End;
        return !(endsBeforeOrAtStart || startsAfterOrAtEnd);
    }))
        return Results.BadRequest(new { error = "This slot is already booked." });

    var price = 500m + (command.Hours - 1) * 300m;
    return Results.Ok(price);
});

app.MapPost("/bookings", async (BookCommand command,
    InMemoryBookingRepository repository, SystemClock clock, FakePaymentGateway payment,
    ConsoleEmailSender email, ConsoleAuditLog audit) =>
{
    var localTime = command.Start;
    if (string.IsNullOrWhiteSpace(command.CustomerEmail) || !command.CustomerEmail.Contains('@'))
        return Results.BadRequest(new { error = "A customer email is required." });
    if (localTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        return Results.BadRequest(new { error = "Kare works Monday to Friday." });
    if (localTime.Minute != 0 || localTime.Second != 0 || localTime.Ticks % TimeSpan.TicksPerSecond != 0)
        return Results.BadRequest(new { error = "Start must be a whole clock hour." });
    if (command.Hours is < 1 or > 4)
        return Results.BadRequest(new { error = "Duration must be 1 to 4 whole hours." });
    if (localTime.Hour < 12 || localTime.Hour + command.Hours > 16)
        return Results.BadRequest(new { error = "The booking must fit within 12:00–16:00." });

    var daysSinceMonday = ((int)localTime.DayOfWeek + 6) % 7;
    var deadline = localTime.Date.AddDays(-daysSinceMonday - 3).AddHours(16);
    if (clock.Now > deadline)
        return Results.BadRequest(new { error = "Book no later than Friday 16:00 in the previous week." });

    var existing = await repository.GetAllAsync();
    if (existing.Any(booking =>
    {
        if (booking.IsCancelled) return false;
        var end = localTime.AddHours(command.Hours);
        var endsBeforeOrAtStart = end <= booking.Start;
        var startsAfterOrAtEnd = localTime >= booking.End;
        return !(endsBeforeOrAtStart || startsAfterOrAtEnd);
    }))
        return Results.BadRequest(new { error = "This slot is already booked." });

    var price = 500m + (command.Hours - 1) * 300m;
    var booking = new Booking(Guid.NewGuid(), command.CustomerEmail, localTime, command.Hours, price);
    await repository.SaveAsync(booking);
    await payment.ChargeAsync(booking.Id, price);
    await email.SendAsync(booking.CustomerEmail, $"Booking {booking.Id} confirmed: {price} NOK");
    await audit.WriteAsync($"BookingCreated {booking.Id}");
    return Results.Created($"/bookings/{booking.Id}", booking);
});

app.MapPost("/bookings/{id:guid}/cancel", async (Guid id,
    InMemoryBookingRepository repository, SystemClock clock, FakePaymentGateway payment,
    ConsoleEmailSender email, ConsoleAuditLog audit) =>
{
    var booking = await repository.GetByIdAsync(id);
    if (booking is null) return Results.NotFound(new { error = "Booking not found." });
    if (booking.IsCancelled) return Results.BadRequest(new { error = "Booking is already cancelled." });

    var remaining = booking.Start - clock.Now;
    if (remaining <= TimeSpan.Zero) return Results.BadRequest(new { error = "Booking has already started." });
    var refund = remaining >= TimeSpan.FromDays(7) ? booking.Price
        : remaining >= TimeSpan.FromHours(48) ? Math.Max(0, booking.Price - 100m) : 0m;
    booking.IsCancelled = true;

    await repository.SaveAsync(booking);
    if (refund > 0) await payment.RefundAsync(booking.Id, refund);
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
