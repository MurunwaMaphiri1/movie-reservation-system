namespace MovieReservationsSystem.Models.DTO;

public class MovieReservationDTO
{
    public int UserId { get; set; }
    public int MovieId { get; set; }
    public DateOnly ReservationDate { get; set; }
    public int TimeSlotId { get; set; }
    public string[] SeatNumbers { get; set; }
}