using KareBooking.V4.Core.DomainModel;

namespace KareBooking.V4.Api.ImperativeShell.Ports;

public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(Guid id);
    Task SaveAsync(Booking booking);
}
