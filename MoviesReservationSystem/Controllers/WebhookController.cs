using System.Text.Json;
using DotNetEnv;
using MoviesReservationSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesReservationSystem.Models.DTO;
using MoviesReservationSystem.Models.Entities;
using MoviesReservationSystem.Services.Email_Service;
using RestSharp.Serializers;
using Stripe;
using Stripe.Checkout;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.IO;
using ApplicationDbContext = MoviesReservationSystem.Data.ApplicationDbContext;

namespace MovieReservationsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WebhookController> _logger;
        private readonly string _webhookSecret;
        private readonly IEmailService _emailService;

        public WebhookController(ApplicationDbContext context, 
            ILogger<WebhookController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _webhookSecret = Env.GetString("STRIPE_WEBHOOK_SECRET");
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = new StreamReader(Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    await json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret
                );

                if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;

                    if (session.Metadata.TryGetValue("reservation_data", out var reservationJson))
                    {
                        await HandleCompletedCheckout(reservationJson);
                    }
                    else
                    {
                        _logger.LogError("No reservation data found in the session metadata.");
                    }
                }

                return Ok();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        }

        private async Task HandleCompletedCheckout(string reservationJson)
        {
            try
            {
                var reservationDetails = JsonSerializer.Deserialize<MovieReservationDTO>
                (
                    reservationJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                var reservation = new MovieReservations()
                {
                    UserId = reservationDetails.UserId,
                    MovieId = reservationDetails.MovieId,
                    ReservationDate = reservationDetails.ReservationDate,
                    TimeSlotId = reservationDetails.TimeSlotId,
                    SeatNumbers = reservationDetails.SeatNumbers,
                };
                
                _context.MovieReservations.Add(reservation);
                await _context.SaveChangesAsync();
                
                var fullReservation = await _context.MovieReservations
                    .Include(r=> r.User)
                    .Include(r=> r.Movie)
                    .Include(r => r.TimeSlot)
                    .FirstOrDefaultAsync(r => r.Id == reservation.Id);
                
                if (fullReservation == null) 
                    throw new Exception($"Reservation with id {reservation.Id} not found.");
                
                // var emailContent = $@"
                //     Dear {reservation.User.FullName},
                //     Your reservation for '{reservation.Movie.Title}' is confirmed!
                //     Date: {reservation.ReservationDate}
                //     Time: {reservation.TimeSlot.TimeSlot}
                //     Seats: {string.Join(", ", reservation.SeatNumbers)}
                //     Total Price: {reservation.GetTotalPrice():C}
                // ";
                var templatePath = Path.Combine("Services", "Email Service", "ReservationConfirmation.html");
                var htmlTemplate = await System.IO.File.ReadAllTextAsync(templatePath);

                var emailBody = htmlTemplate
                    .Replace("{{reservation.User.FullName}}", reservation.User.FullName)
                    .Replace("{{reservation.Movie.Title}}", reservation.Movie.Title)
                    .Replace("{{reservation.ReservationDate}}", reservation.ReservationDate.ToString())
                    .Replace("{{reservation.TimeSlot.TimeSlot}}", reservation.TimeSlot.TimeSlot.ToString())
                    .Replace("{{reservation.SeatNumbers}}", string.Join(",", reservation.SeatNumbers))
                    .Replace("{{reservation.GetTotalPrice():C}}", reservation.GetTotalPrice().ToString("C"));

                await _emailService.SendEmailAsync(fullReservation.User.Email, 
                    "Reservation Confirmation", emailBody);
                
                _logger.LogInformation("Reservation completed for {MovieId}", reservation.MovieId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
