using RealTimeBookingSystem.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RealTimeBookingSystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public Dictionary<int, string> BookedBlocks { get; set; } = new();
        public int TotalBlocks { get; set; } = 100;

        public IndexModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task OnGetAsync()
        {
            // Fetch current state from Redis so the page renders correctly on F5 refresh
            BookedBlocks = await _bookingService.GetBookingsAsync();
        }

        
    }
}
