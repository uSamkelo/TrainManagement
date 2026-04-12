using PaymentService.DTOs;

namespace PaymentService.Services.Interfaces
{
    public interface ITicketService
    {
        Task<PurchaseResult?> PurchaseTicketAsync(Guid userId, PurchaseTicketRequest request);
        Task<TicketDTO?> GetTicketAsync(Guid ticketId);
        Task<List<TicketDTO>> GetUserTicketsAsync(Guid userId);
        Task<List<TicketDTO>> GetActiveUserTicketsAsync(Guid userId);
        Task<bool> ValidateTicketAsync(Guid ticketId);
        Task<bool> CancelTicketAsync(Guid ticketId, Guid userId);
        Task<TicketDTO?> RegenerateQRCodeAsync(Guid ticketId, Guid userId);
    }
}
