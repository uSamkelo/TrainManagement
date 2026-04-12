using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TicketQRService.DTOs;
using TicketQRService.Services.Interfaces;

namespace TicketQRService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketQRController : ControllerBase
    {
        private readonly IQRCodeService _qrCodeService;
        private readonly ITicketImageService _ticketImageService;

        public TicketQRController(IQRCodeService qrCodeService, ITicketImageService ticketImageService)
        {
            _qrCodeService = qrCodeService;
            _ticketImageService = ticketImageService;
        }

        [HttpPost("generate")]
        public IActionResult Generate([FromBody] QRGenerateRequest request)
        {
            var verificationCode = _qrCodeService.GenerateVerificationCode(request.TicketId);

            // QR content contains ticket verification data
            var qrContent = JsonSerializer.Serialize(new
            {
                ticketId = request.TicketId,
                userId = request.UserId,
                route = request.RouteName,
                from = request.FromStation,
                to = request.ToStation,
                type = request.TicketType,
                validFrom = request.ValidFrom.ToString("yyyy-MM-dd"),
                validUntil = request.ValidUntil.ToString("yyyy-MM-dd"),
                code = verificationCode
            });

            var qrCodeBase64 = _qrCodeService.GenerateQRCodeBase64(qrContent);
            var ticketImageBase64 = _ticketImageService.GenerateTicketImageBase64(
                request, qrCodeBase64, verificationCode);

            return Ok(new QRGenerateResponse
            {
                TicketId = request.TicketId,
                QRCodeBase64 = qrCodeBase64,
                TicketImageBase64 = ticketImageBase64,
                VerificationCode = verificationCode
            });
        }

        [HttpGet("verify/{ticketId:guid}")]
        public IActionResult GetVerificationCode(Guid ticketId)
        {
            var code = _qrCodeService.GenerateVerificationCode(ticketId);
            return Ok(new { ticketId, verificationCode = code });
        }
    }
}
