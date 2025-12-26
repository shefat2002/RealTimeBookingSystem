using StackExchange.Redis;

namespace RealTimeBookingSystem.Services
{
    public interface IBookingService
    {
        Task<bool> BookBlockAsync(int blockId, string userName);
        Task<Dictionary<int, string>> GetBookingsAsync();
        Task ResetAllAsync();
    }

    public class BookingService : IBookingService
    {
        private readonly IConnectionMultiplexer _redis;
        private const string RedisKey = "booking_state"; // Changed to Hash

        public BookingService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<bool> BookBlockAsync(int blockId, string userName)
        {
            var db = _redis.GetDatabase();
            // HashSet with When.NotExists acts as an atomic "Book if empty"
            return await db.HashSetAsync(RedisKey, blockId, userName, When.NotExists);
        }

        public async Task<Dictionary<int, string>> GetBookingsAsync()
        {
            var db = _redis.GetDatabase();
            var all = await db.HashGetAllAsync(RedisKey);
            return all.ToDictionary(x => (int)x.Name, x => (string)x.Value);
        }

        public async Task ResetAllAsync()
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(RedisKey);
        }
    }
}
