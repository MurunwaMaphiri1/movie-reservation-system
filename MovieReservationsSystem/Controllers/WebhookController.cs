using System.Text.Json;
using DotNetEnv;
using MovieReservationsSystem.Data;
using Microsoft.AspNetCore.Mvc;
using MovieReservationsSystem.Models.DTO;
using MovieReservationsSystem.Models.Entities;
using RestSharp.Serializers;
using Stripe;
using Stripe.Checkout;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MovieReservationsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WebhookController> _logger;
        private readonly string _webhookSecret;

        public WebhookController(ApplicationDbContext context, 
            ILogger<WebhookController> logger)
        {
            _context = context;
            _logger = logger;
            _webhookSecret = Env.GetString("STRIPE_WEBHOOK_SECRET");
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
                
                _logger.LogInformation("Reservation completed for {MovieId}", reservation.MovieId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
