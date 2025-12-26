using RealTimeBookingSystem.Hubs;
using RealTimeBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace RealTimeBookingSystem.Controllers
{
    public class BookingRequest
    {
        public string UserName { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IHubContext<BookingHub> _hubContext;
        private readonly IBroadcastService _broadcastService;

        public BookingController(IBookingService bookingService, IHubContext<BookingHub> hubContext, IBroadcastService broadcastService)
        {
            _bookingService = bookingService;
            _hubContext = hubContext;
            _broadcastService = broadcastService;
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> BookBlock(int id, [FromBody] BookingRequest request)
        {
            if (id < 1 || id > 100) return BadRequest("Invalid block ID");
            if (string.IsNullOrWhiteSpace(request.UserName)) return BadRequest("User name required");

            // 1. Try to book in Redis
            bool success = await _bookingService.BookBlockAsync(id, request.UserName);

            if (success)
            {
                // 2. If successful, Queue broadcast
                _broadcastService.QueueUpdate(id, request.UserName);
                return Ok(new { success = true, message = $"Block {id} booked by {request.UserName}." });
            }
            else
            {
                return Conflict(new { success = false, message = "Block already taken." });
            }
        }
        
        // Optional: Endpoint to reset for testing
        [HttpPost("reset")]
        public async Task<IActionResult> Reset()
        {
            await _bookingService.ResetAllAsync();
            await _hubContext.Clients.All.SendAsync("ResetAll");
            return Ok();
        }
    }
}
