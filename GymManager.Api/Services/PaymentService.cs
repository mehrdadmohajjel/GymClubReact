using GymManager.Api.Data;
using GymManager.Api.Models;
using Microsoft.EntityFrameworkCore;
using Parbad;
using Parbad.AspNetCore;


namespace GymManager.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _db;
        private readonly IOnlinePayment _onlinePayment;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            AppDbContext db,
            IOnlinePayment onlinePayment,
            IHttpContextAccessor http,
            ILogger<PaymentService> logger)
        {
            _db = db;
            _onlinePayment = onlinePayment;
            _http = http;
            _logger = logger;
        }

        public async Task<(Guid paymentId, string redirectUrl)> CreatePaymentAsync(
            Guid userId, Guid gymId, decimal amount, bool isOnline, HttpContext httpContext)
        {
            try
            {
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    GymId = gymId,
                    UserId = userId,
                    Amount = amount,
                    IsOnline = isOnline,
                    IsPaid = false,
                    PaymentStatus = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _db.Payments.Add(payment);
                await _db.SaveChangesAsync();

                if (!isOnline)
                {
                    payment.IsPaid = true;
                    payment.PaymentStatus = "Completed";
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();

                    return (payment.Id, "/Payment/PaymentResult?paymentId=" + payment.Id + "&success=true");
                }

                // ایجاد درخواست پرداخت آنلاین
                var callbackUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/Payment/Verify?paymentId={payment.Id}";

                var result = await _onlinePayment.RequestAsync(invoice =>
                {
                    invoice.SetCallbackUrl(callbackUrl)
                           .SetAmount(amount)
                           .SetGateway("Melli") // یا از تنظیمات بگیرید
                           .UseAutoIncrementTrackingNumber();
                });

                if (result.IsSucceed)
                {
                    // ذخیره شماره تراکنش
                    payment.TrackingNumber = result.TrackingNumber;
                    payment.GatewayName = result.GatewayName;
                    await _db.SaveChangesAsync();

                    // انتقال به درگاه پرداخت
                    return (payment.Id, result.GatewayTransporter.GetGatewayUrl());
                }
                else
                {
                    _logger.LogError($"Parbad CreatePayment failed: {result.Message}");
                    payment.PaymentStatus = "Failed";
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();

                    throw new Exception($"Unable to create payment: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreatePaymentAsync");
                throw;
            }
        }

        public async Task VerifyCallbackAsync(HttpContext httpContext)
        {
            try
            {
                var invoice = await _onlinePayment.FetchAsync();

                // دریافت paymentId از کوئری استرینگ
                var paymentIdStr = httpContext.Request.Query["paymentId"].ToString();
                if (!Guid.TryParse(paymentIdStr, out var paymentId))
                {
                    throw new Exception("PaymentId missing in callback");
                }

                var payment = await _db.Payments.FindAsync(paymentId);
                if (payment == null)
                {
                    throw new Exception($"Payment {paymentId} not found");
                }

                if (invoice.Status != PaymentFetchResultStatus.ReadyForVerifying)
                {
                    _logger.LogWarning($"Payment {paymentId} is not ready for verification. Status: {invoice.Status}");

                    payment.PaymentStatus = "Failed";
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                    return;
                }

                // تأیید پرداخت
                var verifyResult = await _onlinePayment.VerifyAsync(invoice);

                // به‌روزرسانی پرداخت
                payment.IsPaid = verifyResult.IsSucceed;
                payment.PaymentStatus = verifyResult.IsSucceed ? "Completed" : "Failed";
                payment.TransactionCode = verifyResult.TransactionCode;
                payment.GatewayReference = verifyResult.GatewayReferenceNumber;
                payment.VerifiedAt = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;

                if (verifyResult.IsSucceed)
                {
                    // ایجاد عضویت برای کاربر
                    var membership = new Membership
                    {
                        Id = Guid.NewGuid(),
                        UserId = payment.UserId.Value,
                        GymId = payment.GymId,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(1), // 1 ماه عضویت
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _db.Memberships.Add(membership);
                }

                await _db.SaveChangesAsync();

                _logger.LogInformation($"Payment {paymentId} verified. Success: {verifyResult.IsSucceed}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyCallbackAsync");
                throw;
            }
        }

        public async Task<Payment?> GetPaymentByIdAsync(Guid id)
        {
            return await _db.Payments
                .Include(p => p.User)
                .Include(p => p.Gym)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> ProcessRefundAsync(long trackingNumber, string reason)
        {
            try
            {
                var result = await _onlinePayment.RefundCompletelyAsync(trackingNumber);

                if (result.IsSucceed)
                {
                    var payment = await _db.Payments
                        .FirstOrDefaultAsync(p => p.TrackingNumber == trackingNumber);

                    if (payment != null)
                    {
                        payment.PaymentStatus = "Refunded";
                        payment.RefundReason = reason;
                        payment.RefundedAt = DateTime.UtcNow;
                        payment.UpdatedAt = DateTime.UtcNow;

                        await _db.SaveChangesAsync();
                    }
                }

                return result.IsSucceed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing refund for tracking number {trackingNumber}");
                return false;
            }
        }

        public async Task<List<Payment>> GetUserPaymentsAsync(Guid userId)
        {
            return await _db.Payments
                .Where(p => p.UserId == userId)
                .Include(p => p.Gym)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
