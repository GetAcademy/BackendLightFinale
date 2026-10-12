using KareBooking.V3.DomainModel;

namespace KareBooking.V3.DomainServices;

public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(Guid id);
    Task SaveAsync(Booking booking);
}
