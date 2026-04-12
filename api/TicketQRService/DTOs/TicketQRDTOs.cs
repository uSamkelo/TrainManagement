using System.ComponentModel.DataAnnotations;

namespace TicketQRService.DTOs
{
    public class QRGenerateRequest
    {
        [Required]
        public Guid TicketId { get; set; }
        public Guid UserId { get; set; }
        public string RouteName { get; set; } = null!;
        public string FromStation { get; set; } = null!;
        public string ToStation { get; set; } = null!;
        public string TicketType { get; set; } = null!;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public decimal Price { get; set; }
    }

    public class QRGenerateResponse
    {
        public Guid TicketId { get; set; }
        public string QRCodeBase64 { get; set; } = null!;
        public string TicketImageBase64 { get; set; } = null!;
        public string VerificationCode { get; set; } = null!;
    }
}
