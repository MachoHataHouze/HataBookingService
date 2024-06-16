using HataBookingService.Data.DTOs;
using HataBookingService.Data.Models;

namespace HataBookingService.Data.Services;

public interface IBookingService
{
    Task<IEnumerable<Booking>> GetAllAsync();
    Task<Booking> GetByIdAsync(Guid id);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task DeleteAsync(Guid id);
    Task<Booking> CreateBookingAsync(BookingDto bookingDto, int userId);
    Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId);
}