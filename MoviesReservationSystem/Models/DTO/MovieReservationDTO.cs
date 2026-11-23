using System.ComponentModel.DataAnnotations;

namespace MoviesReservationSystem.Models.DTO;

public class MovieReservationDTO
{
    // Set server-side from the caller's token; any value sent by the client is overwritten
    public int? UserId { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public int MovieId { get; set; }
    public string? ReservationCode { get; set; }
    public string? PaymentId { get; set; }
    public DateOnly ReservationDate { get; set; }
    public int TimeSlotId { get; set; }
    public string[] SeatNumbers { get; set; }
}
