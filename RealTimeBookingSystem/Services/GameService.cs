using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using RealTimeBookingSystem.Hubs;
using RealTimeBookingSystem.Services;

namespace RealTimeBookingSystem.Services
{
    public enum GameState
    {
        WaitingForPlayers,
        CountingDown,
        InProgress,
        Revealing,
        Finished
    }

    public record RewardBlock(int BlockId, string RewardType, int RewardValue);

    public interface IGameService
    {
        Task StartGameAsync();
        Task<GameState> GetGameStateAsync();
        Task<List<RewardBlock>> GetRewardBlocksAsync();
        Task<bool> CanBookBlockAsync();
        Task CheckAutoStartAsync();
        void SetPlayerCount(int count);
    }

    public class GameService : BackgroundService, IGameService
    {
        private readonly IHubContext<BookingHub> _hubContext;
        private readonly IBookingService _bookingService;
        private GameState _gameState = GameState.WaitingForPlayers;
        private List<RewardBlock> _rewardBlocks = new();
        private DateTime _gameStartTime;
        private int _playerCount = 0;
        private readonly object _lockObject = new();
        private const int GameDurationMinutes = 1;
        private const int MinPlayersToStart = 10;
        private const int MaxBlocks = 100;

        public GameService(IHubContext<BookingHub> hubContext, IBookingService bookingService)
        {
            _hubContext = hubContext;
            _bookingService = bookingService;
            GenerateRewardBlocks();
        }

        private void GenerateRewardBlocks()
        {
            var random = new Random();
            _rewardBlocks.Clear();
            
            // Generate 10-15 reward blocks randomly
            var rewardCount = random.Next(10, 16);
            var rewardTypes = new[] { "Coin", "Gem", "Star", "Trophy", "Diamond" };
            var usedBlocks = new HashSet<int>();

            for (int i = 0; i < rewardCount; i++)
            {
                int blockId;
                do
                {
                    blockId = random.Next(1, MaxBlocks + 1);
                } while (usedBlocks.Contains(blockId));

                usedBlocks.Add(blockId);
                var rewardType = rewardTypes[random.Next(rewardTypes.Length)];
                var rewardValue = rewardType switch
                {
                    "Coin" => random.Next(10, 100),
                    "Gem" => random.Next(5, 50),
                    "Star" => random.Next(1, 10),
                    "Trophy" => random.Next(1, 5),
                    "Diamond" => random.Next(1, 3),
                    _ => 1
                };

                _rewardBlocks.Add(new RewardBlock(blockId, rewardType, rewardValue));
            }
        }

        public void SetPlayerCount(int count)
        {
            lock (_lockObject)
            {
                _playerCount = count;
            }
        }

        public async Task CheckAutoStartAsync()
        {
            lock (_lockObject)
            {
                if (_gameState == GameState.WaitingForPlayers && _playerCount >= MinPlayersToStart)
                {
                    _ = Task.Run(StartGameAsync);
                }
            }
        }

        public async Task StartGameAsync()
        {
            lock (_lockObject)
            {
                if (_gameState != GameState.WaitingForPlayers) return;
                _gameState = GameState.CountingDown;
            }

            // Countdown
            await _hubContext.Clients.All.SendAsync("GameCountdown", 3);
            await Task.Delay(1000);
            await _hubContext.Clients.All.SendAsync("GameCountdown", 2);
            await Task.Delay(1000);
            await _hubContext.Clients.All.SendAsync("GameCountdown", 1);
            await Task.Delay(1000);
            await _hubContext.Clients.All.SendAsync("GameCountdown", 0);

            lock (_lockObject)
            {
                _gameState = GameState.InProgress;
                _gameStartTime = DateTime.UtcNow;
            }

            await _hubContext.Clients.All.SendAsync("GameStarted");
        }

        public Task<GameState> GetGameStateAsync()
        {
            lock (_lockObject)
            {
                return Task.FromResult(_gameState);
            }
        }

        public Task<List<RewardBlock>> GetRewardBlocksAsync()
        {
            return Task.FromResult(_rewardBlocks.ToList());
        }

        public Task<bool> CanBookBlockAsync()
        {
            lock (_lockObject)
            {
                return Task.FromResult(_gameState == GameState.InProgress);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);

                lock (_lockObject)
                {
                    if (_gameState == GameState.InProgress)
                    {
                        var elapsed = DateTime.UtcNow - _gameStartTime;
                        if (elapsed.TotalMinutes >= GameDurationMinutes)
                        {
                            _gameState = GameState.Revealing;
                            _ = Task.Run(async () => await RevealRewardsAsync());
                        }
                    }
                }
            }
        }

        private async Task RevealRewardsAsync()
        {
            await _hubContext.Clients.All.SendAsync("GameEnded");

            // Get current bookings to determine winners
            var bookings = await _bookingService.GetBookingsAsync();
            var winners = new List<(string UserName, RewardBlock Reward)>();

            foreach (var reward in _rewardBlocks)
            {
                if (bookings.TryGetValue(reward.BlockId, out var userName))
                {
                    winners.Add((userName, reward));
                }
            }

            // Send rewards revelation
            await _hubContext.Clients.All.SendAsync("RewardsRevealed", _rewardBlocks);

            // Send winner notifications
            foreach (var (userName, reward) in winners)
            {
                await _hubContext.Clients.All.SendAsync("WinnerNotification", userName, reward);
            }

            lock (_lockObject)
            {
                _gameState = GameState.Finished;
            }

            // Reset for next game after 10 seconds
            await Task.Delay(10000);
            await ResetGameAsync();
        }

        private async Task ResetGameAsync()
        {
            await _bookingService.ResetAllAsync();
            GenerateRewardBlocks();

            lock (_lockObject)
            {
                _gameState = GameState.WaitingForPlayers;
            }

            await _hubContext.Clients.All.SendAsync("GameReset");
        }
    }
}