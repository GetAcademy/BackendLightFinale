using KareBooking.V4.DomainModel;
using KareBooking.V4.DomainServices;

namespace KareBooking.V4.Infrastructure;

public sealed class InMemoryBookingRepository : IBookingRepository
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
