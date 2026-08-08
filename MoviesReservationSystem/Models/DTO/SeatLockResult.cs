namespace MoviesReservationSystem.Models.DTO
{
    public class SeatLockResult
    {
        public List<String> Locked { get; set; } = new();
        public List<String> Failed { get; set; } = new();
    }
}
