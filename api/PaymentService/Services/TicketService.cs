using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.DTOs;
using PaymentService.Models;
using PaymentService.Services.Interfaces;

namespace PaymentService.Services
{
    public class TicketService : ITicketService
    {
        private readonly PaymentContext _context;
        private readonly IPaymentProcessingService _paymentProcessing;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public TicketService(
            PaymentContext context,
            IPaymentProcessingService paymentProcessing,
            IConfiguration config,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _paymentProcessing = paymentProcessing;
            _config = config;
            _httpClient = httpClientFactory.CreateClient("TrainTracking");
        }

        public async Task<PurchaseResult?> PurchaseTicketAsync(Guid userId, PurchaseTicketRequest request)
        {
            if (!Enum.TryParse<TicketType>(request.TicketType, true, out var ticketType))
                return null;

            var price = GetPrice(ticketType);
            var (validFrom, validUntil) = GetValidityPeriod(ticketType, request.TravelDate);

            // Fetch route name from TrainTrackingService
            var routeName = await FetchRouteNameAsync(request.RouteId) ?? request.RouteId;

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = ticketType,
                RouteId = request.RouteId,
                RouteName = routeName,
                FromStation = request.FromStation,
                ToStation = request.ToStation,
                ValidFrom = validFrom,
                ValidUntil = validUntil,
                Price = price
            };

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                UserId = userId,
                Amount = price,
                PaymentMethod = request.PaymentMethod
            };

            _context.Tickets.Add(ticket);
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Process payment
            var paymentSuccess = await _paymentProcessing.ProcessPaymentAsync(
                payment.Id, request.PaymentMethod, price);

            if (paymentSuccess)
            {
                payment.Status = PaymentStatus.Completed;
                payment.CompletedAt = DateTime.UtcNow;
                payment.TransactionRef = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
                ticket.Status = TicketStatus.Active;

                // Request QR code generation from TicketQRService
                ticket.QRCode = await RequestQRCodeAsync(ticket);
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                ticket.Status = TicketStatus.Cancelled;
            }

            await _context.SaveChangesAsync();

            return new PurchaseResult
            {
                Ticket = MapTicketToDTO(ticket),
                Payment = MapPaymentToDTO(payment)
            };
        }

        public async Task<TicketDTO?> GetTicketAsync(Guid ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            return ticket == null ? null : MapTicketToDTO(ticket);
        }

        public async Task<List<TicketDTO>> GetUserTicketsAsync(Guid userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => MapTicketToDTO(t))
                .ToListAsync();
        }

        public async Task<List<TicketDTO>> GetActiveUserTicketsAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            return await _context.Tickets
                .Where(t => t.UserId == userId
                    && t.Status == TicketStatus.Active
                    && t.ValidUntil >= now)
                .OrderBy(t => t.ValidFrom)
                .Select(t => MapTicketToDTO(t))
                .ToListAsync();
        }

        public async Task<bool> ValidateTicketAsync(Guid ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            var now = DateTime.UtcNow;
            if (ticket.Status != TicketStatus.Active) return false;
            if (now < ticket.ValidFrom || now > ticket.ValidUntil) return false;

            // For once-off tickets, mark as used after validation
            if (ticket.Type == TicketType.OnceOff)
            {
                ticket.Status = TicketStatus.Used;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> CancelTicketAsync(Guid ticketId, Guid userId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Payment)
                .FirstOrDefaultAsync(t => t.Id == ticketId && t.UserId == userId);

            if (ticket == null || ticket.Status != TicketStatus.Active) return false;

            ticket.Status = TicketStatus.Cancelled;

            if (ticket.Payment != null && ticket.Payment.Status == PaymentStatus.Completed)
            {
                await _paymentProcessing.RefundPaymentAsync(ticket.Payment.Id);
                ticket.Payment.Status = PaymentStatus.Refunded;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TicketDTO?> RegenerateQRCodeAsync(Guid ticketId, Guid userId)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.UserId == userId);
            if (ticket == null || ticket.Status != TicketStatus.Active) return null;

            ticket.QRCode = await RequestQRCodeAsync(ticket);
            if (ticket.QRCode != null)
            {
                await _context.SaveChangesAsync();
            }
            return MapTicketToDTO(ticket);
        }

        private decimal GetPrice(TicketType type)
        {
            return type switch
            {
                TicketType.OnceOff => decimal.Parse(_config["TicketPricing:OnceOff"] ?? "15", System.Globalization.CultureInfo.InvariantCulture),
                TicketType.Weekly => decimal.Parse(_config["TicketPricing:Weekly"] ?? "75", System.Globalization.CultureInfo.InvariantCulture),
                TicketType.Monthly => decimal.Parse(_config["TicketPricing:Monthly"] ?? "250", System.Globalization.CultureInfo.InvariantCulture),
                _ => 15m
            };
        }

        private static (DateTime validFrom, DateTime validUntil) GetValidityPeriod(
            TicketType type, DateTime? travelDate)
        {
            var now = DateTime.UtcNow;
            return type switch
            {
                TicketType.OnceOff => (
                    travelDate?.Date ?? now.Date,
                    (travelDate?.Date ?? now.Date).AddDays(1).AddTicks(-1)),
                TicketType.Weekly => (now.Date, now.Date.AddDays(7).AddTicks(-1)),
                TicketType.Monthly => (now.Date, now.Date.AddMonths(1).AddTicks(-1)),
                _ => (now.Date, now.Date.AddDays(1).AddTicks(-1))
            };
        }

        private async Task<string?> FetchRouteNameAsync(string routeId)
        {
            try
            {
                var url = _config["Services:TrainTrackingServiceUrl"];
                var response = await _httpClient.GetFromJsonAsync<RouteResponse>(
                    $"{url}/api/trainroute/{routeId}");
                return response?.LineName;
            }
            catch
            {
                return null;
            }
        }

        private async Task<string?> RequestQRCodeAsync(Ticket ticket)
        {
            try
            {
                var url = _config["Services:TicketQRServiceUrl"];
                var response = await _httpClient.PostAsJsonAsync($"{url}/api/ticketqr/generate", new
                {
                    ticketId = ticket.Id,
                    userId = ticket.UserId,
                    routeName = ticket.RouteName,
                    fromStation = ticket.FromStation,
                    toStation = ticket.ToStation,
                    ticketType = ticket.Type.ToString(),
                    validFrom = ticket.ValidFrom,
                    validUntil = ticket.ValidUntil,
                    price = ticket.Price
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<QRCodeResponse>();
                    return result?.QRCodeBase64;
                }
            }
            catch
            {
                // QR service unavailable — ticket still valid, QR can be regenerated
            }
            return null;
        }

        private static TicketDTO MapTicketToDTO(Ticket t) => new()
        {
            Id = t.Id,
            Type = t.Type.ToString(),
            Status = t.Status.ToString(),
            RouteId = t.RouteId,
            RouteName = t.RouteName,
            FromStation = t.FromStation,
            ToStation = t.ToStation,
            ValidFrom = t.ValidFrom,
            ValidUntil = t.ValidUntil,
            Price = t.Price,
            QRCode = t.QRCode,
            CreatedAt = t.CreatedAt
        };

        private static PaymentDTO MapPaymentToDTO(Payment p) => new()
        {
            Id = p.Id,
            TicketId = p.TicketId,
            Amount = p.Amount,
            Currency = p.Currency,
            Status = p.Status.ToString(),
            PaymentMethod = p.PaymentMethod,
            TransactionRef = p.TransactionRef,
            CreatedAt = p.CreatedAt,
            CompletedAt = p.CompletedAt
        };

        // Internal DTOs for inter-service communication
        private class RouteResponse { public string? LineName { get; set; } }
        private class QRCodeResponse { public string? QRCodeBase64 { get; set; } }
    }
}
