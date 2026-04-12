using SkiaSharp;
using TicketQRService.DTOs;
using TicketQRService.Services.Interfaces;

namespace TicketQRService.Services
{
    public class TicketImageService : ITicketImageService
    {
        public string GenerateTicketImageBase64(
            QRGenerateRequest request, string qrCodeBase64, string verificationCode)
        {
            const int width = 600;
            const int height = 900;
            const int margin = 30;

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            // Background
            canvas.Clear(SKColors.White);

            // Draw border
            using var borderPaint = new SKPaint
            {
                Color = new SKColor(0x2C, 0x3E, 0x50),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3
            };
            canvas.DrawRoundRect(new SKRoundRect(new SKRect(10, 10, width - 10, height - 10), 15, 15), borderPaint);

            // Header bar
            using var headerPaint = new SKPaint
            {
                Color = new SKColor(0x66, 0x7E, 0xEA),
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRoundRect(new SKRoundRect(new SKRect(10, 10, width - 10, 100), 15, 15), headerPaint);
            canvas.DrawRect(new SKRect(10, 80, width - 10, 100), headerPaint);

            // Header text
            using var headerFont = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 28);
            using var whiteTextPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
            canvas.DrawText("CAPE TOWN METRORAIL", margin + 10, 65, SKTextAlign.Left, headerFont, whiteTextPaint);

            // Ticket type badge
            using var badgePaint = new SKPaint
            {
                Color = GetTicketTypeColor(request.TicketType),
                Style = SKPaintStyle.Fill
            };
            var badgeRect = new SKRect(margin, 120, margin + 140, 155);
            canvas.DrawRoundRect(new SKRoundRect(badgeRect, 8, 8), badgePaint);

            using var badgeFont = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 18);
            canvas.DrawText(request.TicketType.ToUpperInvariant(), margin + 10, 143, SKTextAlign.Left, badgeFont, whiteTextPaint);

            // Route info
            var y = 190f;
            DrawLabel(canvas, "ROUTE", margin, y);
            DrawValue(canvas, request.RouteName, margin, y + 22);

            y += 60;
            DrawLabel(canvas, "FROM", margin, y);
            DrawValue(canvas, request.FromStation, margin, y + 22);

            y += 60;
            DrawLabel(canvas, "TO", margin, y);
            DrawValue(canvas, request.ToStation, margin, y + 22);

            // Arrow between stations
            using var arrowFont = new SKFont(SKTypeface.Default, 30);
            using var arrowPaint = new SKPaint { Color = new SKColor(0x66, 0x7E, 0xEA), IsAntialias = true };
            canvas.DrawText("↓", width / 2 - 10, 285, SKTextAlign.Left, arrowFont, arrowPaint);

            // Dashed divider
            y += 70;
            using var dashPaint = new SKPaint
            {
                Color = new SKColor(0xBD, 0xC3, 0xC7),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                PathEffect = SKPathEffect.CreateDash([8, 4], 0)
            };
            canvas.DrawLine(margin, y, width - margin, y, dashPaint);

            // Validity dates
            y += 30;
            DrawLabel(canvas, "VALID FROM", margin, y);
            DrawValue(canvas, request.ValidFrom.ToString("dd MMM yyyy"), margin, y + 22);

            DrawLabel(canvas, "VALID UNTIL", width / 2, y);
            DrawValue(canvas, request.ValidUntil.ToString("dd MMM yyyy"), width / 2, y + 22);

            // Price
            y += 60;
            DrawLabel(canvas, "PRICE", margin, y);
            using var priceFont = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 26);
            using var pricePaint = new SKPaint { Color = new SKColor(0x27, 0xAE, 0x60), IsAntialias = true };
            canvas.DrawText($"R {request.Price:F2}", margin, y + 28, SKTextAlign.Left, priceFont, pricePaint);

            // Verification code
            DrawLabel(canvas, "VERIFICATION CODE", width / 2, y);
            using var codeFont = new SKFont(SKTypeface.FromFamilyName("Courier New", SKFontStyle.Bold), 20);
            using var codePaint = new SKPaint { Color = new SKColor(0x2C, 0x3E, 0x50), IsAntialias = true };
            canvas.DrawText(verificationCode, width / 2, y + 28, SKTextAlign.Left, codeFont, codePaint);

            // Second dashed divider
            y += 60;
            canvas.DrawLine(margin, y, width - margin, y, dashPaint);

            // QR Code
            y += 20;
            var qrBytes = Convert.FromBase64String(qrCodeBase64);
            using var qrBitmap = SKBitmap.Decode(qrBytes);
            if (qrBitmap != null)
            {
                var qrSize = 200;
                var qrX = (width - qrSize) / 2f;
                var destRect = new SKRect(qrX, y, qrX + qrSize, y + qrSize);
                canvas.DrawBitmap(qrBitmap, destRect);
            }

            // Scan instruction
            y += 220;
            using var scanFont = new SKFont(SKTypeface.Default, 14);
            using var scanPaint = new SKPaint { Color = new SKColor(0x7F, 0x8C, 0x8D), IsAntialias = true };
            canvas.DrawText("Scan QR code at station for entry", width / 2f, y, SKTextAlign.Center, scanFont, scanPaint);

            // Ticket ID footer
            y += 25;
            using var footerFont = new SKFont(SKTypeface.FromFamilyName("Courier New", SKFontStyle.Normal), 11);
            using var footerPaint = new SKPaint { Color = new SKColor(0xBD, 0xC3, 0xC7), IsAntialias = true };
            canvas.DrawText($"Ticket ID: {request.TicketId}", width / 2f, y, SKTextAlign.Center, footerFont, footerPaint);

            // Encode to PNG
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 90);
            return Convert.ToBase64String(data.ToArray());
        }

        private static void DrawLabel(SKCanvas canvas, string text, float x, float y)
        {
            using var font = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal), 12);
            using var paint = new SKPaint { Color = new SKColor(0x7F, 0x8C, 0x8D), IsAntialias = true };
            canvas.DrawText(text, x, y, SKTextAlign.Left, font, paint);
        }

        private static void DrawValue(SKCanvas canvas, string text, float x, float y)
        {
            using var font = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 20);
            using var paint = new SKPaint { Color = new SKColor(0x2C, 0x3E, 0x50), IsAntialias = true };
            canvas.DrawText(text, x, y, SKTextAlign.Left, font, paint);
        }

        private static SKColor GetTicketTypeColor(string ticketType) => ticketType.ToLowerInvariant() switch
        {
            "onceoff" => new SKColor(0xE6, 0x7E, 0x22),
            "weekly" => new SKColor(0x27, 0xAE, 0x60),
            "monthly" => new SKColor(0x29, 0x80, 0xB9),
            _ => new SKColor(0x66, 0x7E, 0xEA)
        };
    }
}
