using KareBooking.V0.DomainModel;

namespace KareBooking.V0.DomainServices;

public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(Guid id);
    Task SaveAsync(Booking booking);
}
