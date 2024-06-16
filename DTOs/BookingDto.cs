namespace HataBookingService.Data.DTOs;

public class BookingDto
{
    public Guid PropertyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
