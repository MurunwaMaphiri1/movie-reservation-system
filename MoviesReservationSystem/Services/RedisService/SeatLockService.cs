using MoviesReservationSystem.Models.DTO;
using StackExchange.Redis;

namespace MoviesReservationSystem.Services.RedisService
{
    public class SeatLockService : ISeatLockService
    {
        private readonly IConnectionMultiplexer _redis;

        public SeatLockService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<List<string>> LockSeats(SeatLockDTO seatLockDTO)
        {
            var db = _redis.GetDatabase();
            var failedLocks = new List<string>();

            foreach (var seat in seatLockDTO.SeatNumbers)
            {
                var key = $"seat:lock:{seatLockDTO.MovieId}:{seatLockDTO.Date}:{seatLockDTO.TimeSlotId}:{seat}";
        
                Console.WriteLine($"Attempting to lock key: {key}");
        
                bool locked = await db.StringSetAsync(key, seatLockDTO.UserId.ToString(),
                    TimeSpan.FromMinutes(10), When.NotExists);

                Console.WriteLine($"Lock result for {key}: {locked}");

                if (!locked)
                    failedLocks.Add(seat);
            }

            return failedLocks;
        }

        public async Task UnlockSeats(SeatLockDTO seatLockDTO)
        {
            var db = _redis.GetDatabase();

            foreach (var seat in seatLockDTO.SeatNumbers)
            {
                var key = $"seat:lock:{seatLockDTO.MovieId}:{seatLockDTO.Date}:{seatLockDTO.TimeSlotId}:{seat}";
                await db.KeyDeleteAsync(key);
            }
        }

        public async Task<List<string>> GetLockedSeats(int movieId, DateOnly date, int timeSlotId)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var pattern = $"seat:lock:{movieId}:{date}:{timeSlotId}:*";
            
            return server.Keys(pattern: pattern)
                .Select(k => k.ToString().Split(':').Last())
                .ToList();
        }
    }    
}

