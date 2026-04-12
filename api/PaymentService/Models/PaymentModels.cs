namespace PaymentService.Models
{
    public enum TicketType
    {
        OnceOff,
        Weekly,
        Monthly
    }

    public enum TicketStatus
    {
        Active,
        Used,
        Expired,
        Cancelled,
        Refunded
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public class Ticket
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public TicketType Type { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.Active;
        public string RouteId { get; set; } = null!;
        public string RouteName { get; set; } = null!;
        public string FromStation { get; set; } = null!;
        public string ToStation { get; set; } = null!;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public decimal Price { get; set; }
        public string? QRCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Payment? Payment { get; set; }
    }

    public class Payment
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public Ticket? Ticket { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "ZAR";
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string PaymentMethod { get; set; } = null!; // card, eft, cash
        public string? TransactionRef { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
