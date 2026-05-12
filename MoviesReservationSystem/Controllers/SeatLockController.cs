using Microsoft.AspNetCore.Mvc;
using MoviesReservationSystem.Data;
using MoviesReservationSystem.Models.DTO;
using MoviesReservationSystem.Services.RedisService;
using StackExchange.Redis;

namespace MoviesReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeatLockController : ControllerBase
    {
        private readonly ILogger<SeatLockController> _logger;
        private readonly ISeatLockService _seatLockService;

        public SeatLockController(ILogger<SeatLockController> logger, ISeatLockService seatLockService)
        {
            _logger = logger;
            _seatLockService = seatLockService;
        }
        
        // Get Locked Seats
        [HttpGet("locked-seats")]
        public async Task<IActionResult> GetLockedSeats([FromQuery] int movieId, [FromQuery] DateOnly date,
            [FromQuery] int timeSlotId)
        {
            var lockedSeats = _seatLockService.GetLockedSeats(movieId, date, timeSlotId);
            return Ok(lockedSeats);
        }
        
        // Lock Seat
        /*[HttpPost("lock")]
        public async Task<IActionResult> LockSeatSelection([FromBody] SeatLockDTO seatLockDTO)
        {
            var db = _redis.GetDatabase();
            var failedLocks = new List<string>();
            
            foreach (var seat in seatLockDTO.SeatNumbers)
            {
                var pattern =
                    $"seat:lock:{seatLockDTO.MovieId}:{seatLockDTO.Date}:{seatLockDTO.TimeSlotId}:{seatLockDTO.SeatNumbers}";
                bool locked = await db.StringSetAsync(pattern, seatLockDTO.UserId.ToString(), 
                    TimeSpan.FromMinutes(10), When.NotExists);

                if (!locked)
                {
                    failedLocks.Add(seat);
                }
            }
            if (failedLocks.Any())
                return Conflict(new { message = "Some seats are already locked", seats = failedLocks });

            return Ok(new { message = "Seats locked successfully." });
        }*/
        [HttpPost("lock")]
        public async Task<IActionResult> LockSeatSelection([FromBody] SeatLockDTO seatLockDTO)
        {
            var failedLocks = await _seatLockService.LockSeats(seatLockDTO);
            
            if (failedLocks.Any()) 
                return Conflict(new { message = "Some seats are already locked", seats = failedLocks });

            return Ok(new { message = "Seats locked successfully" });
        }
        
        // Unlock seat
        /*[HttpDelete("unlock")]
        public async Task<IActionResult> UnlockSeatSelection([FromBody] SeatLockDTO seatLockDTO)
        {
            var db = _redis.GetDatabase();

            foreach (var seat in seatLockDTO.SeatNumbers)
            {
                var key =
                    $"seat:lock:{seatLockDTO.MovieId}:{seatLockDTO.Date}{seatLockDTO.TimeSlotId}{seatLockDTO.SeatNumbers}";
                await db.KeyDeleteAsync(key);
            }

            return Ok(new { message = "Seats unlocked successfully" });
        }*/
        [HttpDelete("unlock")]
        public async Task<IActionResult> UnlockSeatSelection([FromBody] SeatLockDTO seatLockDTO)
        {
            await _seatLockService.UnlockSeats(seatLockDTO);
            return Ok(new { message = "Seats unlocked successfully" });
        }
    }    
}

