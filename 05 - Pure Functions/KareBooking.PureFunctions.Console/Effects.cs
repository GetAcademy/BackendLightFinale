namespace KareBooking.PureFunctions.Console;

// A fact: the domain has decided that a booking was created.
// It is not a request to save, charge or send anything.
public sealed record BookingCreated(Guid BookingId, string CustomerEmail);

// Planned actions: describing an effect does not execute it.
public abstract record Effect;
public sealed record SaveBooking(BookingValue Booking) : Effect;
public sealed record ChargePayment(Guid BookingId, decimal Amount) : Effect;
public sealed record SendEmail(string Address, string Message) : Effect;

public static class EffectExamples
{
    // Pure: ordinary data in, ordinary data out. No I/O takes place here.
    public static IReadOnlyList<Effect> Plan(BookingValue booking, string customerEmail) =>
    [
        new SaveBooking(booking),
        new ChargePayment(booking.Id, booking.Price),
        new SendEmail(customerEmail, "Booking confirmed")
    ];

    // Effectful interpreter. Console simulates the adapters; no real database or network.
    public static void Interpret(IReadOnlyList<Effect> effects)
    {
        foreach (var effect in effects)
        {
            switch (effect)
            {
                case SaveBooking save:
                    System.Console.WriteLine($"[repository] Save {save.Booking.Id}");
                    break;
                case ChargePayment charge:
                    System.Console.WriteLine($"[payment gateway] Charge {charge.Amount} for {charge.BookingId}");
                    break;
                case SendEmail email:
                    System.Console.WriteLine($"[email sender] To {email.Address}: {email.Message}");
                    break;
            }
        }
    }
}
