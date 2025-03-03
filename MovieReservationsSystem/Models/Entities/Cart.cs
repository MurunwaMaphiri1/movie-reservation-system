using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieReservationsSystem.Models.Entities;

public class Cart
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

    [Required] public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddMinutes(15);
    
    public bool IsExpired => DateTime.UtcNow > ExpirationDate;
}