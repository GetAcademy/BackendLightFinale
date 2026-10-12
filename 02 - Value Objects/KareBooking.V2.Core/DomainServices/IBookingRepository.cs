using KareBooking.V2.DomainModel;

namespace KareBooking.V2.DomainServices;

public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(Guid id);
    Task SaveAsync(Booking booking);
}
