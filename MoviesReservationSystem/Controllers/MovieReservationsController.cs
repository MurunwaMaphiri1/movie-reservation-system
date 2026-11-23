using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MoviesReservationSystem.Data;
using MoviesReservationSystem.Models.Entities;
using Stripe;
using DotNetEnv;
using Microsoft.Extensions.Caching.Distributed;
using MoviesReservationSystem.Models.DTO;
using Newtonsoft.Json;
using System.Text.Json;
using MoviesReservationSystem.Services.EmailService;
using Stripe.Checkout;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MoviesReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieReservationsController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MovieReservationsController> _logger;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;


        public MovieReservationsController(ApplicationDbContext context, 
            ILogger<MovieReservationsController> logger, IEmailService emailService,
            IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            DotNetEnv.Env.Load();
            StripeConfiguration.ApiKey = Env.GetString("STRIPE_SECRET_KEY");
            _emailService = emailService;
            _env = env;
        }

        // Employee tokens also carry an "Id" claim, but it refers to the Employees table, not Users
        private int? GetCustomerUserId() =>
            User.FindFirst("AccountType")?.Value == "User"
            && int.TryParse(User.FindFirst("Id")?.Value, out var id)
                ? id
                : null;
        
        //Get all Movie reservations
        [HttpGet]
        [EnableRateLimiting("browsing")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> GetMovieReservations()
        {
            var allMovieRes = _context.MovieReservations.ToList();
            return Ok(allMovieRes);
        }
        
        //Get taken seats
        [HttpGet("taken-seats/{movieId}/{date}/{timeSlotId}")]
        [EnableRateLimiting("browsing")]
        public async Task<IActionResult> GetTakenSeats([FromRoute] DateOnly date, [FromRoute] int timeSlotId, 
            [FromRoute] int movieId)
        {
            var reservedSeats = _context.MovieReservations
                .Where(r => r.ReservationDate == date && r.TimeSlotId == timeSlotId 
                                                      && r.MovieId == movieId)
                .SelectMany(r => r.SeatNumbers)
                .ToList();
            
            return Ok(reservedSeats);
                
        }

        [HttpPost("create-checkout")]
        [EnableRateLimiting("booking")]
        public async Task<IActionResult> CreateCheckout([FromBody] MovieReservationDTO movieReservation)
        {
            // Guests have no token, so UserId stays null; a client-supplied UserId is never trusted
            movieReservation.UserId = GetCustomerUserId();

            try
            {
                // Check for seat conflicts
                var existingReservation = _context.MovieReservations
                    .Where(r => r.MovieId == movieReservation.MovieId &&
                                r.ReservationDate == movieReservation.ReservationDate &&
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
        
        //Get the logged-in user's reservations
        [HttpGet("my-reservations")]
        [EnableRateLimiting("search")]
        [Authorize]
        public async Task<IActionResult> GetMyReservations()
        {
            if (GetCustomerUserId() is not int userId)
                return Forbid();

            var reservations = await _context.MovieReservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Movie)
                .Include(r => r.TimeSlot)
                .ToListAsync();

            if (reservations == null || reservations.Count == 0)
            {
                return NotFound("No reservations found for this user."); 
            }

            return Ok(reservations);
        }
        
        //Get reservation by ReservationCode
        [HttpGet("search-by-rescode")]
        [EnableRateLimiting("search")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> GetMovieReservationByCode([FromQuery] string code)
        {
            var reservation = await _context.MovieReservations
                .FirstOrDefaultAsync(r => r.ReservationCode == code);

            if (reservation == null)
            {
                return NotFound("No reservations found for this code.");
            }
            
            return Ok(reservation);
        }
        
        //Cancel Reservation
        [HttpDelete("delete")]
        [EnableRateLimiting("booking")]
        public async Task<IActionResult> DeleteMovieReservation([FromQuery] string code, [FromQuery] string email)
        {
            try
            {
                // Code + email together prove ownership, for guests and account holders alike
                var reservation = await _context.MovieReservations
                    .Include(r => r.Movie)
                    .Include(r => r.TimeSlot)
                    .FirstOrDefaultAsync(r => r.ReservationCode == code
                                              && r.Email.ToLower() == email.ToLower());

                if (reservation == null)
                {
                    return NotFound("Reservation Not Found");
                }

                var templatePath =
                    Path.Combine(_env.ContentRootPath, "Services", "EmailService", "ReservationCancellationConfirmation.html");
                var htmlTemplate = await System.IO.File.ReadAllTextAsync(templatePath);

                var emailBody = htmlTemplate
                    .Replace("{{reservation.User.FullName}}", reservation.FullName)
                    .Replace("{{reservation.Movie.Title}}", reservation.Movie.Title)
                    .Replace("{{reservation.Id}}", reservation.Id.ToString())
                    .Replace("{{reservation.ReservationDate}}", reservation.ReservationDate.ToString())
                    .Replace("{{reservation.TimeSlot.TimeSlot}}", reservation.TimeSlot.TimeSlot.ToString())
                    .Replace("{{reservation.SeatNumbers}}", string.Join(",", reservation.SeatNumbers))
                    .Replace("{{reservation.ReservationID}}", reservation.ReservationCode)
                    .Replace("{{reservation.GetTotalPrice():C}}", reservation.GetTotalPrice().ToString("C"));

                await _emailService.SendEmailAsync(reservation.Email,
                    "Reservation Cancellation Confirmation", emailBody);

                StripeConfiguration.ApiKey = Env.GetString("STRIPE_SECRET_KEY");

                var options = new RefundCreateOptions
                {
                    PaymentIntent = reservation.PaymentId,
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _context.MovieReservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
            return NoContent();
        }
    }
}
