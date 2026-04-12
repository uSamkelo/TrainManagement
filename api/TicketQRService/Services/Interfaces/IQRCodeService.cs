using TicketQRService.DTOs;

namespace TicketQRService.Services.Interfaces
{
    public interface IQRCodeService
    {
        string GenerateQRCodeBase64(string content, int pixelsPerModule = 10);
        string GenerateVerificationCode(Guid ticketId);
    }
}
