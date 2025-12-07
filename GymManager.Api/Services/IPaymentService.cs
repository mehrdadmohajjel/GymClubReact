using GymManager.Api.Models;

namespace GymManager.Api.Services
{
    public interface IPaymentService
    {
        Task<(Guid paymentId, string redirectUrl)> CreatePaymentAsync(Guid userId, Guid gymId, decimal amount, bool isOnline, HttpContext httpContext);
        Task VerifyCallbackAsync(HttpContext httpContext);
        Task<Payment?> GetPaymentByIdAsync(Guid id);
        Task<bool> ProcessRefundAsync(long trackingNumber, string reason);
        Task<List<Payment>> GetUserPaymentsAsync(Guid userId);
    }

}
