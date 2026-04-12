using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs
{
    public class PurchaseTicketRequest
    {
        [Required]
        public string TicketType { get; set; } = null!; // OnceOff, Weekly, Monthly

        [Required]
        public string RouteId { get; set; } = null!;

        [Required]
        public string FromStation { get; set; } = null!;

        [Required]
        public string ToStation { get; set; } = null!;

        [Required]
        public string PaymentMethod { get; set; } = null!; // card, eft, cash

        public DateTime? TravelDate { get; set; } // For once-off tickets
    }

    public class TicketDTO
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string RouteId { get; set; } = null!;
        public string RouteName { get; set; } = null!;
        public string FromStation { get; set; } = null!;
        public string ToStation { get; set; } = null!;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public decimal Price { get; set; }
        public string? QRCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PaymentDTO
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
        public string? TransactionRef { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class PurchaseResult
    {
        public TicketDTO Ticket { get; set; } = null!;
        public PaymentDTO Payment { get; set; } = null!;
    }

    public class TicketAvailabilityDTO
    {
        public string RouteId { get; set; } = null!;
        public string RouteName { get; set; } = null!;
        public List<string> Stations { get; set; } = new();
        public Dictionary<string, decimal> Pricing { get; set; } = new();
        public bool Available { get; set; }
    }
}
