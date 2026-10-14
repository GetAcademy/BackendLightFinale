using KareBooking.PreludeA.Api.DomainModel;

namespace KareBooking.PreludeA.Api.Infrastructure;

public sealed class InMemoryBookingRepository
{
    private readonly Dictionary<Guid, Booking> bookings = [];

    public Task<IReadOnlyList<Booking>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<Booking>>(bookings.Values.Select(b => b.Copy()).ToArray());
    }

    public Task<Booking?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(bookings.TryGetValue(id, out var booking) ? booking.Copy() : null);
    }

    public Task SaveAsync(Booking booking)
    {
        bookings[booking.Id] = booking.Copy();
        return Task.CompletedTask;
    }
}

