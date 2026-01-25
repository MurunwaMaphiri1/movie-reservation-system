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
using Stripe.Webhooks.Module;
using ApplicationDbContext = MoviesReservationSystem.Data.ApplicationDbContext;

namespace MoviesReservationSystem.Controllers
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
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                     json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret
                );

                if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;

                    if (session.Metadata.TryGetValue("reservation_data", out var reservationJson))
                    {
                        await HandleCompletedCheckout(reservationJson, session.PaymentIntentId);
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
        
        private async Task HandleCompletedCheckout(string reservationJson, string paymentId)
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
                    PaymentId = paymentId
                };
                
                var exists = await _context.MovieReservations
                    .AnyAsync(r => r.PaymentId == reservation.PaymentId);

                if (exists)
                {
                    _logger.LogInformation("Reservation already exists for payment {PaymentId}");
                    return;
                }
                
                _context.MovieReservations.Add(reservation);
                await _context.SaveChangesAsync();
                
                var fullReservation = await _context.MovieReservations
                    .Include(r=> r.User)
                    .Include(r=> r.Movie)
                    .Include(r => r.TimeSlot)
                    .FirstOrDefaultAsync(r => r.Id == reservation.Id);
                
                if (fullReservation == null) 
                    throw new Exception($"Reservation with id {reservation.Id} not found.");
                
                
                var templatePath = Path.Combine("Services", "Email Service", "ReservationConfirmation.html");
                var htmlTemplate = await System.IO.File.ReadAllTextAsync(templatePath);
                
                var emailBody = htmlTemplate
                    .Replace("{{reservation.User.FullName}}", fullReservation.User.FullName)
                    .Replace("{{reservation.Movie.Title}}", fullReservation.Movie.Title)
                    .Replace("{{reservation.Id}}",  fullReservation.Id.ToString())
                    .Replace("{{reservation.ReservationDate}}", fullReservation.ReservationDate.ToString())
                    .Replace("{{reservation.TimeSlot.TimeSlot}}", fullReservation.TimeSlot.TimeSlot.ToString())
                    .Replace("{{reservation.SeatNumbers}}", string.Join(",", fullReservation.SeatNumbers))
                    .Replace("{{reservation.ReservationID}}", reservation.Id.ToString())
                    .Replace("{{reservation.GetTotalPrice():C}}", fullReservation.GetTotalPrice().ToString("C"));
                
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
