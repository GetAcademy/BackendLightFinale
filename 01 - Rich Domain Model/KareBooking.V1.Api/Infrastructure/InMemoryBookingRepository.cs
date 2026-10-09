namespace KareBooking.V1;

public sealed class InMemoryBookingRepository : IBookingRepository
{
    private readonly Dictionary<Guid, Booking> bookings = [];
    private readonly object gate = new();

    public Task<IReadOnlyList<Booking>> GetAllAsync()
    {
        lock (gate) return Task.FromResult<IReadOnlyList<Booking>>(bookings.Values.Select(b => b.Copy()).ToArray());
    }

    public Task<Booking?> GetByIdAsync(Guid id)
    {
        lock (gate) return Task.FromResult(bookings.TryGetValue(id, out var booking) ? booking.Copy() : null);
    }

    public Task SaveAsync(Booking booking)
    {
        lock (gate) bookings[booking.Id] = booking.Copy();
        return Task.CompletedTask;
    }
}
