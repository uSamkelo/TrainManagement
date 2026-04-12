using TicketQRService.DTOs;

namespace TicketQRService.Services.Interfaces
{
    public interface ITicketImageService
    {
        string GenerateTicketImageBase64(QRGenerateRequest request, string qrCodeBase64, string verificationCode);
    }
}
