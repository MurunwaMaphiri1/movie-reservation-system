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
        private readonly ILogger _logger;
        


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
        
        // //Create reservation
        // [HttpPost("/create-reservation")]
        // public async Task<IActionResult> CreateReservation([FromBody] MovieReservations reservation)
        // {
        //     
        //     //Check if seats are taken if a user books within the same timeslot as someone else
        //     var existingReservation = _context.MovieReservations
        //         .Where(r => r.MovieId == reservation.MovieId && r.ReservationDate == reservation.ReservationDate && r.TimeSlotId == reservation.TimeSlotId)
        //         .SelectMany(r => r.SeatNumbers)
        //         .ToList();
        //
        //     var conflictingSeats = reservation.SeatNumbers.Intersect(existingReservation).ToList();
        //
        //     if (conflictingSeats.Any())
        //     {
        //         return BadRequest(new
        //         {
        //             _logger = "Some seats are already booked",
        //             ConflictingSeats = conflictingSeats
        //         });
        //     }
        //
        //     var newReservation = new MovieReservations
        //     {
        //         UserId = reservation.UserId,
        //         MovieId = reservation.MovieId,
        //         ReservationDate = reservation.ReservationDate,
        //         TimeSlotId = reservation.TimeSlotId,
        //         SeatNumbers = reservation.SeatNumbers
        //     };
        //     
        //     _context.MovieReservations.Add(newReservation);
        //     await _context.SaveChangesAsync();
        //     return Ok( new
        //     {
        //         _logger = "Reservation created",
        //         newReservation,
        //         TotalPrice = newReservation.GetTotalPrice()
        //     });
        // }

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

                
                var reservationData = new MovieReservationDTO()
                {
                    UserId = movieReservation.UserId,
                    MovieId = movieReservation.MovieId,
                    ReservationDate = movieReservation.ReservationDate,
                    TimeSlotId = movieReservation.TimeSlotId,
                    SeatNumbers = movieReservation.SeatNumbers,
                    // TotalPrice = movieReservation.SeatNumbers.Length * movie.TicketPrice
                };
                

                // Create Stripe checkout session
                StripeConfiguration.ApiKey = Env.GetString("STRIPE_SECRET_KEY");

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
                    SuccessUrl = $"{Request.Scheme}://{Request.Host}/confirm-reservation",
                    CancelUrl = $"{Request.Scheme}://{Request.Host}/cancelled-reservation"
                };

                var service = new SessionService();
                var session = service.Create(options);
                
                

                return Ok(new { reservationData, sessionId = session.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating checkout session");
                return StatusCode(500, $"Error creating checkout session: {ex.Message}");
            }
        }

        // [HttpGet("confirm-reservation")]
        // public async Task<IActionResult> ConfirmReservation(string id)
        // {
        //     try
        //     {
        //         // Get reservation data from Redis
        //         string reservationJson = await _distributedCache.GetStringAsync($"reservation:{id}");
        //         
        //         if (string.IsNullOrEmpty(reservationJson))
        //         {
        //             return NotFound("Reservation data not found or expired");
        //         }
        //
        //         var tempReservation = JsonSerializer.Deserialize<MovieReservationDTO>(reservationJson);
        //
        //         // Create the actual reservation
        //         var newReservation = new MovieReservations
        //         {
        //             UserId = tempReservation.UserId,
        //             MovieId = tempReservation.MovieId,
        //             ReservationDate = tempReservation.ReservationDate,
        //             TimeSlotId = tempReservation.TimeSlotId,
        //             SeatNumbers = tempReservation.SeatNumbers
        //         };
        //
        //         _context.MovieReservations.Add(newReservation);
        //         await _context.SaveChangesAsync();
        //
        //         // Remove the temp reservation from Redis
        //         await _distributedCache.RemoveAsync($"reservation:{id}");
        //
        //         // Return a view or redirect to a confirmation page
        //         return Ok(new 
        //         { 
        //             message = "Reservation confirmed", 
        //             reservationId = newReservation.Id,
        //             totalPaid = tempReservation.TotalPrice
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error confirming reservation");
        //         return StatusCode(500, $"Error confirming reservation: {ex.Message}");
        //     }
        // }
        
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
