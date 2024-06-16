using HataBookingService.Data.Models;

namespace HataBookingService.Data.Repositories;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>> GetAllAsync();
    Task<Booking> GetByIdAsync(Guid id);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId);
}