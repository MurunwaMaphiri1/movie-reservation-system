using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesReservationSystem.Models.Entities;

public class TimeSlots
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TimeSlotId { get; set; }
    
    public TimeOnly TimeSlot { get; set; }
}