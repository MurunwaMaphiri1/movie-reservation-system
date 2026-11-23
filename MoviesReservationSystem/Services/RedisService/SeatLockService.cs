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

        private const string LockScript = @"
            local locked = {}
            for i, key in ipairs(KEYS) do
                if redis.call('SET', key, ARGV[1], 'NX', 'EX', ARGV[2]) then
                    table.insert(locked, key)
                end
            end
            return locked";

        // Only releases keys whose stored value matches the caller's holder id
        private const string UnlockScript = @"
            for i, key in ipairs(KEYS) do
                if redis.call('GET', key) == ARGV[1] then
                    redis.call('DEL', key)
                end
            end
            return 1";

        public async Task UnlockSeats(SeatLockDTO dto)
        {
            var db = _redis.GetDatabase();
            var keys = dto.SeatNumbers.Select(seat => (RedisKey)$"seat:lock:{dto.MovieId}:{dto.Date}:{dto.TimeSlotId}:{seat}").ToArray();

            await db.ScriptEvaluateAsync(UnlockScript, keys, new RedisValue[] { dto.HolderId });
        }

        public async Task<SeatLockResult> LockSeats(SeatLockDTO dto)
        {
            var db = _redis.GetDatabase();
            var keys = dto.SeatNumbers.Select(seat => (RedisKey)$"seat:lock:{dto.MovieId}:{dto.Date}:{dto.TimeSlotId}:{seat}").ToArray();

            var lockedKeys = (RedisResult[])await db.ScriptEvaluateAsync(
                LockScript, keys, new RedisValue[] { dto.HolderId, 600 });

            var lockedSeats = lockedKeys
                .Select(k => ((string)k).Split(':').Last())
                .ToHashSet();
            var failedSeats = dto.SeatNumbers
                .Where(seat => !lockedSeats.Contains(seat))
                .ToList();

            return new SeatLockResult { Locked = lockedSeats.ToList(), Failed = failedSeats };
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
