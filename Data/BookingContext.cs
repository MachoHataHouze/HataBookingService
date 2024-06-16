using HataBookingService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HataBookingService.Data.Data;

public class BookingContext : DbContext
{
    public BookingContext(DbContextOptions<BookingContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings { get; set; }
}