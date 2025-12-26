using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using RealTimeBookingSystem.Hubs;

namespace RealTimeBookingSystem.Services
{
    public interface IBroadcastService
    {
        void QueueUpdate(int blockId, string userName);
    }

    public record BookingUpdate(int BlockId, string UserName);

    public class BroadcastService : BackgroundService, IBroadcastService
    {
        private readonly IHubContext<BookingHub> _hubContext;
        private readonly ConcurrentQueue<BookingUpdate> _updates = new();
        private readonly PeriodicTimer _timer;
        private const int BroadcastIntervalMs = 200;

        public BroadcastService(IHubContext<BookingHub> hubContext)
        {
            _hubContext = hubContext;
            _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(BroadcastIntervalMs));
        }

        public void QueueUpdate(int blockId, string userName)
        {
            _updates.Enqueue(new BookingUpdate(blockId, userName));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                if (_updates.IsEmpty) continue;

                var batch = new List<BookingUpdate>();
                while (_updates.TryDequeue(out var update))
                {
                    batch.Add(update);
                }

                if (batch.Any())
                {
                    await _hubContext.Clients.All.SendAsync("BatchBlockBooked", batch, stoppingToken);
                }
            }
        }
    }
}
