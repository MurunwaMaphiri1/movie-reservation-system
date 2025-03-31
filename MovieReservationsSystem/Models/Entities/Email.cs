using System.ComponentModel.DataAnnotations;

namespace MovieReservationsSystem.Models.Entities;

public class Email
{
    [Required]
    public string Sender { get; set; }
    
    [Required]
    public string Receiver { get; set; }
    
    [Required]
    public string Subject { get; set; }
    
    [Required]
    public string Body { get; set; }
}