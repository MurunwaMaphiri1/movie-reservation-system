using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieReservationsSystem.Models.Entities;

public class MovieReservations
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    public Users User { get; set; }
    
    [Required]
    public int MovieId { get; set; }
    public Movies Movie { get; set; }
    
    [Required]
    public DateTime ReservationDate { get; set; }
    
    [Required]
    public int TimeSlotId { get; set; }
    public TimeSlots TimeSlot { get; set; }
    
    [Required]
    public string[] SeatNumbers { get; set; } = Array.Empty<string>();

    //Calculates the TotalPrice of tickets based off of how many seats the user has selected
    public decimal GetTotalPrice()
    {
        if (Movie == null) return 0;
        return SeatNumbers.Length * Movie.TicketPrice;
    }
}