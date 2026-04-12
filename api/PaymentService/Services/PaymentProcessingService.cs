using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.Services.Interfaces;

namespace PaymentService.Services
{
    public class PaymentProcessingService : IPaymentProcessingService
    {
        private readonly PaymentContext _context;
        private readonly ILogger<PaymentProcessingService> _logger;

        public PaymentProcessingService(PaymentContext context, ILogger<PaymentProcessingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> ProcessPaymentAsync(Guid paymentId, string paymentMethod, decimal amount)
        {
            // Simulated payment processing
            // In production: integrate with a payment gateway (Payfast, Peach Payments, etc.)
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null) return false;

            _logger.LogInformation(
                "Processing {Method} payment of {Amount} ZAR for payment {PaymentId}",
                paymentMethod, amount, paymentId);

            // Simulate processing delay would happen externally
            // For now, all payments succeed
            payment.Status = PaymentStatus.Completed;
            payment.CompletedAt = DateTime.UtcNow;
            payment.TransactionRef = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RefundPaymentAsync(Guid paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null || payment.Status != PaymentStatus.Completed) return false;

            _logger.LogInformation("Refunding payment {PaymentId}", paymentId);

            payment.Status = PaymentStatus.Refunded;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
