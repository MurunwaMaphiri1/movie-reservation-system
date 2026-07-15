namespace MoviesReservationSystem.Models.DTO
{
    public class SeatLockDTO
    {
        public int MovieId { get; set; }
        public DateOnly Date { get; set; }
        public int TimeSlotId { get; set; }
        public string[] SeatNumbers { get; set; }
        public int UserId { get; set; }
    }    
}

