using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieReservationsSystem.Models.Entities;

public class Movies
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public string Genres { get; set; } = "[]";
    
    [Required]
    public string Image { get; set; }
    
    public DateOnly ReleaseDate { get; set; }

    public string Duration { get; set; } = "N/A";
    
    [Required]
    public string DirectedBy { get; set; }

    [Required] 
    public string[] Actors { get; set; } = Array.Empty<string>();
    
    [Required]
    public long TicketPrice { get; set; }
    
    public string Trailer { get; set; }
}