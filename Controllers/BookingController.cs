using System.Security.Claims;
using HataBookingService.Data.DTOs;
using HataBookingService.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HataBookingService.Data.Controllers;

[Route("api/[controller]")]
    [ApiController]
    ///[Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _bookingService.GetAllAsync();
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(Guid id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingDto bookingDto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new { Message = "nameid claim not found" });
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { Message = "Invalid nameid value" });
            }

            var booking = await _bookingService.CreateBookingAsync(bookingDto, userId);
            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(Guid id, [FromBody] BookingDto bookingDto)
        {
            var existingBooking = await _bookingService.GetByIdAsync(id);
            if (existingBooking == null)
            {
                return NotFound();
            }

            existingBooking.PropertyId = bookingDto.PropertyId;
            existingBooking.StartDate = bookingDto.StartDate;
            existingBooking.EndDate = bookingDto.EndDate;
            // Recalculate total price and regenerate contract if needed
            existingBooking.TotalPrice = (bookingDto.EndDate - bookingDto.StartDate).Days * await GetPropertyPriceAsync(bookingDto.PropertyId);
            existingBooking.Contract = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Updated contract content"));

            await _bookingService.UpdateAsync(existingBooking);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(Guid id)
        {
            var existingBooking = await _bookingService.GetByIdAsync(id);
            if (existingBooking == null)
            {
                return NotFound();
            }

            await _bookingService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Gets all bookings for the authenticated user.
        /// </summary>
        /// <returns>List of bookings for the authenticated user.</returns>
        [HttpGet("mybookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new { Message = "nameid claim not found" });
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { Message = "Invalid nameid value" });
            }

            var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);
            return Ok(bookings);
        }
        
        [HttpGet("mycontracts")]
        public async Task<IActionResult> GetMyContracts()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new { Message = "nameid claim not found" });
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { Message = "Invalid nameid value" });
            }

            var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);
            if (bookings == null || !bookings.Any())
            {
                return NotFound(new { Message = "No contracts found for this user." });
            }

            var contracts = bookings.Select(b => new
            {
                b.Contract // Base64 encoded contract
            });

            return Ok(contracts);
        }
        
        

        private async Task<decimal> GetPropertyPriceAsync(Guid propertyId)
        {
            // Placeholder for actual property price fetching logic
            return 100; // Assume 100 currency units per day for example
        }
    }