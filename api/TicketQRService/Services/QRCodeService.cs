using System.Security.Cryptography;
using System.Text;
using QRCoder;
using TicketQRService.Services.Interfaces;

namespace TicketQRService.Services
{
    public class QRCodeService : IQRCodeService
    {
        public string GenerateQRCodeBase64(string content, int pixelsPerModule = 10)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
            using var pngQrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = pngQrCode.GetGraphic(pixelsPerModule);
            return Convert.ToBase64String(qrCodeBytes);
        }

        public string GenerateVerificationCode(Guid ticketId)
        {
            var input = $"{ticketId}-{DateTime.UtcNow:yyyyMMdd}";
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexStringLower(hash)[..12].ToUpperInvariant();
        }
    }
}
