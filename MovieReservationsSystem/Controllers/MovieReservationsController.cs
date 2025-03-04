using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieReservationsSystem.Data;
using MovieReservationsSystem.Models.Entities;
using MovieReservationsSystem.Services;
using Stripe;
using DotNetEnv;
using Microsoft.Extensions.Caching.Distributed;
using MovieReservationsSystem.Models.DTO;
using Newtonsoft.Json;
using System.Text.Json;
using Stripe.Checkout;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MovieReservationsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieReservationsController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MovieReservationsController> _logger;
        


        public MovieReservationsController(ApplicationDbContext context, 
            ILogger<MovieReservationsController> logger)
        {
            _context = context;
            _logger = logger;
            DotNetEnv.Env.Load();
            StripeConfiguration.ApiKey = Env.GetString("STRIPE_SECRET_KEY");
        }
        
        //Get all Movie reservations
        [HttpGet]
        public async Task<IActionResult> GetMovieReservations()
        {
            var allMovieRes = _context.MovieReservations.ToList();
            return Ok(allMovieRes);
        }

        [HttpPost("create-checkout")]
        public async Task<IActionResult> CreateCheckout([FromBody] MovieReservationDTO movieReservation)
        {
            try
            {
                // Check for seat conflicts
                var existingReservation = _context.MovieReservations
                    .Where(r => r.MovieId == movieReservation.MovieId &&
                                r.ReservationDate.Date == movieReservation.ReservationDate.Date &&
                                r.TimeSlotId == movieReservation.TimeSlotId)
                    .SelectMany(r => r.SeatNumbers)
                    .ToList();

                var conflictingSeats = movieReservation.SeatNumbers.Intersect(existingReservation).ToList();

                if (conflictingSeats.Any())
                {
                    return BadRequest(new
                    {
                        message = "Some seats are already booked",
                        ConflictingSeats = conflictingSeats
                    });
                }

                // Get the movie to calculate price
                var movie = await _context.Movies.FindAsync(movieReservation.MovieId);
                if (movie == null) return NotFound("Movie not found");
                
                // Create Stripe checkout session
                StripeConfiguration.ApiKey = Env.GetString("STRIPE_SECRET_KEY");
                
                var reservationJson = JsonConvert.SerializeObject(movieReservation);

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)((movieReservation.SeatNumbers.Length * movie.TicketPrice) * 100),
                                Currency = "zar",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Images = new List<string> {movie.Image},
                                    Name = $"Tickets for {movie.Title}",
                                    Description = $"{movieReservation.SeatNumbers.Length} seat(s): " +
                                                  $"{string.Join(", ", movieReservation.SeatNumbers)}"
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = $"http://localhost:5173/confirm-reservation",
                    CancelUrl = $"http://localhost:5173/cancelled-reservation",
                    Metadata = new Dictionary<string, string>
                    {
                        { "reservation_data", reservationJson }
                    }
                };

                var service = new SessionService();
                var session = service.Create(options);
                
                

                return Ok(new { sessionId = session.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating checkout session");
                return StatusCode(500, $"Error creating checkout session: {ex.Message}");
            }
        }
        
        //Get movie reservations by UserId
        [HttpGet("search")] 
        public async Task<IActionResult> GetMovieReservationByUserId([FromQuery] int userId)
        {
            var reservations = await _context.MovieReservations
                .Where(r => r.UserId == userId)
                .Include(r => r.User) 
                .Include(r => r.Movie) 
                .Include(r => r.ReservationDate)
                .Include(r => r.TimeSlot)
                .Include(r => r.SeatNumbers.Length)
                .ToListAsync();

            if (reservations == null || reservations.Count == 0)
            {
                return NotFound("No reservations found for this user."); 
            }

            return Ok(reservations);
        }
        
        //Cancel Reservation
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMovieReservation([FromQuery] int id)
        {
            var reservation = await _context.MovieReservations.FindAsync(id);
            _context.MovieReservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
