using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.Services.Interfaces;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpPost("purchase")]
        public async Task<IActionResult> Purchase([FromBody] PurchaseTicketRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _ticketService.PurchaseTicketAsync(userId.Value, request);
            if (result == null)
                return BadRequest(new { error = "Invalid ticket type or purchase failed." });

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyTickets()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            return Ok(await _ticketService.GetUserTicketsAsync(userId.Value));
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetMyActiveTickets()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            return Ok(await _ticketService.GetActiveUserTicketsAsync(userId.Value));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetTicket(Guid id)
        {
            var ticket = await _ticketService.GetTicketAsync(id);
            if (ticket == null) return NotFound();
            return Ok(ticket);
        }

        [HttpPost("{id:guid}/validate")]
        [Authorize(Roles = "Driver,StationStaff,Admin")]
        public async Task<IActionResult> ValidateTicket(Guid id)
        {
            var valid = await _ticketService.ValidateTicketAsync(id);
            return Ok(new { ticketId = id, valid });
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelTicket(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _ticketService.CancelTicketAsync(id, userId.Value);
            if (!result)
                return BadRequest(new { error = "Ticket cannot be cancelled." });
            return Ok(new { message = "Ticket cancelled and refund initiated." });
        }

        [HttpPost("{id:guid}/regenerate-qr")]
        public async Task<IActionResult> RegenerateQR(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var ticket = await _ticketService.RegenerateQRCodeAsync(id, userId.Value);
            if (ticket == null) return NotFound();
            return Ok(ticket);
        }

        [HttpGet("availability")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailability(
            [FromServices] IConfiguration config,
            [FromServices] IHttpClientFactory httpClientFactory)
        {
            var pricing = new Dictionary<string, decimal>
            {
                ["OnceOff"] = decimal.Parse(config["TicketPricing:OnceOff"] ?? "15", System.Globalization.CultureInfo.InvariantCulture),
                ["Weekly"] = decimal.Parse(config["TicketPricing:Weekly"] ?? "75", System.Globalization.CultureInfo.InvariantCulture),
                ["Monthly"] = decimal.Parse(config["TicketPricing:Monthly"] ?? "250", System.Globalization.CultureInfo.InvariantCulture)
            };

            var availability = new List<TicketAvailabilityDTO>();

            try
            {
                var client = httpClientFactory.CreateClient("TrainTracking");
                var routes = await client.GetFromJsonAsync<List<RouteResponse>>("/api/trainroute");
                if (routes != null)
                {
                    foreach (var route in routes)
                    {
                        availability.Add(new TicketAvailabilityDTO
                        {
                            RouteId = route.TrainId,
                            RouteName = route.LineName,
                            Stations = route.Stops?.Select(s => s.StationName).ToList() ?? new(),
                            Pricing = pricing,
                            Available = true
                        });
                    }
                }
            }
            catch
            {
                // If TrainTrackingService is unavailable, return empty availability
            }

            return Ok(availability);
        }

        private class RouteResponse
        {
            public string TrainId { get; set; } = null!;
            public string LineName { get; set; } = null!;
            public List<StopResponse>? Stops { get; set; }
        }

        private class StopResponse
        {
            public string StationName { get; set; } = null!;
        }

        private Guid? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null && Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}
