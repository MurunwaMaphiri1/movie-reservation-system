namespace MovieReservationsSystem.Models.DTO;

public class CartDTO
{
    public int UserId { get; set; }
    public int MovieId { get; set; }
    public DateTime ReservationDate { get; set; }
    public int TimeSlotId { get; set; }
    public string[] SeatNumbers { get; set; } = Array.Empty<string>();
}