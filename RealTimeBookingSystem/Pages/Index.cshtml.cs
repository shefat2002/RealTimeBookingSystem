using RealTimeBookingSystem.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RealTimeBookingSystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public List<int> BookedBlocks { get; set; } = new List<int>();
        public int TotalBlocks { get; set; } = 100;

        public IndexModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task OnGetAsync()
        {
            // Fetch current state from Redis so the page renders correctly on F5 refresh
            var booked = await _bookingService.GetBookedBlocksAsync();
            BookedBlocks = booked.ToList();
        }

        
    }
}
