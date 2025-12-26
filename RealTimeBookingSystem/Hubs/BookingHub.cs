using Microsoft.AspNetCore.SignalR;
using RealTimeBookingSystem.Services;

namespace RealTimeBookingSystem.Hubs
{
    public class BookingHub : Hub
    {
        // Simple in-memory tracking for demo purposes. 
        // In production with multiple server instances, use Redis Sets.
        private static readonly Dictionary<string, string> OnlineUsers = new();
        private readonly IGameService _gameService;

        public BookingHub(IGameService gameService)
        {
            _gameService = gameService;
        }

        public async Task Join(string userName)
        {
            lock (OnlineUsers)
            {
                OnlineUsers[Context.ConnectionId] = userName;
            }

            var uniqueUsers = OnlineUsers.Values.Distinct().ToList();
            _gameService.SetPlayerCount(uniqueUsers.Count);
            
            await Clients.All.SendAsync("UpdateUserList", uniqueUsers);
            await _gameService.CheckAutoStartAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(Context.ConnectionId))
                {
                    OnlineUsers.Remove(Context.ConnectionId);
                }
            }

            var uniqueUsers = OnlineUsers.Values.Distinct().ToList();
            _gameService.SetPlayerCount(uniqueUsers.Count);
            
            await Clients.All.SendAsync("UpdateUserList", uniqueUsers);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
