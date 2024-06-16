namespace HataBookingService.Data.Models;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PropertyId { get; set; }
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Contract { get; set; } // Base64 encoded contract
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}
