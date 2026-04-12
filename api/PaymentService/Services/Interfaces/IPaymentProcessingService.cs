using PaymentService.DTOs;

namespace PaymentService.Services.Interfaces
{
    public interface IPaymentProcessingService
    {
        Task<bool> ProcessPaymentAsync(Guid paymentId, string paymentMethod, decimal amount);
        Task<bool> RefundPaymentAsync(Guid paymentId);
    }
}
