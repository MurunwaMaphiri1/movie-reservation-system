using MoviesReservationSystem.Models.DTO;

namespace MoviesReservationSystem.Services.RedisService
{
    public interface ISeatLockService
    {
        Task<SeatLockResult> LockSeats(SeatLockDTO seatLockDTO);
        Task UnlockSeats(SeatLockDTO seatLockDTO);
        Task<List<string>> GetLockedSeats(int movieId, DateOnly date, int timeSlotId);
    }
}
