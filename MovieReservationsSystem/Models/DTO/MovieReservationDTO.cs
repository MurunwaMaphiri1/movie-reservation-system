namespace MovieReservationsSystem.Models.DTO;

public class MovieReservationDTO
{
    public int UserId { get; set; }
    public int MovieId { get; set; }
    public DateTime ReservationDate { get; set; }
    public int TimeSlotId { get; set; }
    public string[] SeatNumbers { get; set; }
    public decimal TotalPrice { get; set; }
}