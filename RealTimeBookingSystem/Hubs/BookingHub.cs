using Microsoft.AspNetCore.SignalR;

namespace RealTimeBookingSystem.Hubs
{
    public class BookingHub : Hub
    {
        // Simple in-memory tracking for demo purposes. 
        // In production with multiple server instances, use Redis Sets.
        private static readonly Dictionary<string, string> OnlineUsers = new();

        public async Task Join(string userName)
        {
            lock (OnlineUsers)
            {
                OnlineUsers[Context.ConnectionId] = userName;
            }
            
            await Clients.All.SendAsync("UpdateUserList", OnlineUsers.Values.Distinct().ToList());
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
            
            await Clients.All.SendAsync("UpdateUserList", OnlineUsers.Values.Distinct().ToList());
            await base.OnDisconnectedAsync(exception);
        }
    }
}
