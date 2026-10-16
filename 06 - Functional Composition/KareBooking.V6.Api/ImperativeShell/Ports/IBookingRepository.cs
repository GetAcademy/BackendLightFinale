using KareBooking.V6.Core.DomainModel;

namespace KareBooking.V6.Api.ImperativeShell.Ports;

public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(Guid id);
    Task SaveAsync(Booking booking);
}
