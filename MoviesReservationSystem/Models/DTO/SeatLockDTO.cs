using System.ComponentModel.DataAnnotations;

namespace MoviesReservationSystem.Models.DTO
{
    public class SeatLockDTO
    {
        public int MovieId { get; set; }
        public DateOnly Date { get; set; }
        public int TimeSlotId { get; set; }
        public string[] SeatNumbers { get; set; }

        // Random id the frontend generates once per browser session; identifies who holds the lock
        [Required]
        public string HolderId { get; set; } = string.Empty;
    }
}

