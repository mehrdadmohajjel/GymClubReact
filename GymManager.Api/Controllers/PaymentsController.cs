using GymManager.Api.Data;
using GymManager.Api.Models;
using GymManager.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Parbad;

namespace GymManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IOnlinePayment _onlinePayment;
        private readonly PaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IOnlinePayment onlinePayment,
            PaymentService paymentService,
            ILogger<PaymentController> logger)
        {
            _onlinePayment = onlinePayment;
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Pay()
        {
            return View(new PayViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Pay(PayViewModel viewModel)
        {
            try
            {
                // دریافت اطلاعات کاربر از سیستم احراز هویت
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return RedirectToAction("Login", "Account");
                }

                // ذخیره اطلاعات پرداخت در دیتابیس (پیش‌پرداخت)
                var paymentId = Guid.NewGuid();

                // در واقع اینجا باید GymId را از جایی دریافت کنید
                // برای مثال از session یا کوئری استرینگ
                var gymId = GetCurrentGymId();

                // ایجاد URL بازگشت
                var callbackUrl = Url.Action("Verify", "Payment",
                    new { paymentId = paymentId },
                    Request.Scheme);

                // درخواست پرداخت به پارباد
                var result = await _onlinePayment.RequestAsync(invoice =>
                {
                    invoice.SetCallbackUrl(callbackUrl)
                           .SetAmount(viewModel.Amount)
                           .SetGateway(viewModel.SelectedGateway);

                    if (viewModel.GenerateTrackingNumberAutomatically)
                    {
                        invoice.UseAutoIncrementTrackingNumber();
                    }
                    else if (viewModel.TrackingNumber.HasValue)
                    {
                        invoice.SetTrackingNumber(viewModel.TrackingNumber.Value);
                    }
                });

                // ذخیره اطلاعات تراکنش در دیتابیس
                var payment = new Payment
                {
                    Id = paymentId,
                    UserId = userId,
                    GymId = gymId,
                    Amount = viewModel.Amount,
                    IsOnline = true,
                    IsPaid = false,
                    TrackingNumber = result.TrackingNumber,
                    GatewayName = viewModel.SelectedGateway,
                    CreatedAt = DateTime.UtcNow,
                    PaymentStatus = "Pending"
                };

                // باید اینجا payment را به دیتابیس اضافه کنید
                // _dbContext.Payments.Add(payment);
                // await _dbContext.SaveChangesAsync();

                if (result.IsSucceed)
                {
                    _logger.LogInformation($"Payment {paymentId} created successfully. Redirecting to gateway.");
                    return result.GatewayTransporter.TransportToGateway();
                }

                _logger.LogError($"Payment {paymentId} failed. Status: {result.Status}");
                return View("PayRequestError", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Pay method");
                ModelState.AddModelError("", "خطا در ایجاد پرداخت. لطفا مجددا تلاش کنید.");
                return View(viewModel);
            }
        }

        [HttpGet, HttpPost]
        public async Task<IActionResult> Verify(Guid paymentId)
        {
            try
            {
                // دریافت اطلاعات تراکنش از پارباد
                var invoice = await _onlinePayment.FetchAsync();

                // بررسی وضعیت تراکنش
                if (invoice.Status != PaymentFetchResultStatus.ReadyForVerifying)
                {
                    // بررسی اگر تراکنش قبلا پرداخت شده
                    var isAlreadyVerified = invoice.IsAlreadyVerified;

                    _logger.LogWarning($"Payment {paymentId} is not ready for verification. Status: {invoice.Status}");

                    // دریافت اطلاعات پرداخت از دیتابیس
                    var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
                    if (payment != null)
                    {
                        payment.PaymentStatus = "Failed";
                        payment.UpdatedAt = DateTime.UtcNow;
                        // به‌روزرسانی در دیتابیس
                    }

                    TempData["ErrorMessage"] = "پرداخت ناموفق بود یا قبلا انجام شده است.";
                    return RedirectToAction("PaymentResult", new { paymentId = paymentId, success = false });
                }

                // بررسی اینکه آیا هنوز ظرفیت وجود دارد (برای باشگاه)
                if (!IsGymCapacityAvailable(invoice.TrackingNumber))
                {
                    var cancelResult = await _onlinePayment.CancelAsync(
                        invoice,
                        cancellationReason: "ظرفیت باشگاه تکمیل شده است.");

                    _logger.LogInformation($"Payment {paymentId} cancelled due to capacity issues.");

                    // به‌روزرسانی وضعیت در دیتابیس
                    var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
                    if (payment != null)
                    {
                        payment.PaymentStatus = "Cancelled";
                        payment.CancellationReason = "ظرفیت تکمیل شد";
                        payment.UpdatedAt = DateTime.UtcNow;
                        // به‌روزرسانی در دیتابیس
                    }

                    return View("CancelResult", cancelResult);
                }

                // تأیید پرداخت
                var verifyResult = await _onlinePayment.VerifyAsync(invoice);

                // ذخیره اطلاعات تراکنش در دیتابیس
                var dbPayment = await _paymentService.GetPaymentByIdAsync(paymentId);
                if (dbPayment != null)
                {
                    dbPayment.IsPaid = verifyResult.IsSucceed;
                    dbPayment.TransactionCode = verifyResult.TransactionCode;
                    dbPayment.PaymentStatus = verifyResult.IsSucceed ? "Completed" : "Failed";
                    dbPayment.GatewayReference = verifyResult.GatewayReferenceNumber;
                    dbPayment.VerifiedAt = DateTime.UtcNow;
                    dbPayment.UpdatedAt = DateTime.UtcNow;
                    // به‌روزرسانی در دیتابیس

                    if (verifyResult.IsSucceed)
                    {
                        // فعال‌سازی عضویت کاربر
                        await ActivateUserMembership(dbPayment.UserId, dbPayment.GymId);
                    }
                }

                _logger.LogInformation($"Payment {paymentId} verification result: {verifyResult.Status}");

                // ذخیره نتیجه در TempData برای نمایش
                TempData["PaymentResult"] = verifyResult.IsSucceed;
                TempData["TrackingNumber"] = invoice.TrackingNumber;
                TempData["TransactionCode"] = verifyResult.TransactionCode;

                return View("VerifyResult", verifyResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying payment {paymentId}");
                return View("Error", new ErrorViewModel
                {
                    Message = "خطا در تأیید پرداخت"
                });
            }
        }

        [HttpGet]
        public IActionResult PaymentResult(Guid paymentId, bool success)
        {
            // دریافت اطلاعات پرداخت از دیتابیس
            var payment = _paymentService.GetPaymentByIdAsync(paymentId).Result;

            var model = new PaymentResultViewModel
            {
                PaymentId = paymentId,
                IsSuccessful = success,
                TrackingNumber = payment?.TrackingNumber ?? 0,
                Amount = payment?.Amount ?? 0,
                Message = success ? "پرداخت با موفقیت انجام شد." : "پرداخت ناموفق بود."
            };

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Refund()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Refund(RefundViewModel viewModel)
        {
            try
            {
                var result = await _onlinePayment.RefundCompletelyAsync(viewModel.TrackingNumber);

                if (result.IsSucceed)
                {
                    // به‌روزرسانی وضعیت در دیتابیس
                    var payment = await GetPaymentByTrackingNumber(viewModel.TrackingNumber);
                    if (payment != null)
                    {
                        payment.PaymentStatus = "Refunded";
                        payment.UpdatedAt = DateTime.UtcNow;
                        // به‌روزرسانی در دیتابیس
                    }
                }

                return View("RefundResult", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error refunding payment {viewModel.TrackingNumber}");
                ModelState.AddModelError("", "خطا در بازپرداخت. لطفا مجددا تلاش کنید.");
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentHistory()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return RedirectToAction("Login", "Account");

            // دریافت تاریخچه پرداخت‌های کاربر
            var payments = await GetUserPayments(userId);
            return View(payments);
        }

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            // دریافت ID کاربر از سیستم احراز هویت
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (Guid.TryParse(userIdClaim, out Guid userId))
                return userId;
            return Guid.Empty;
        }

        private Guid GetCurrentGymId()
        {
            // دریافت GymId از session یا کوئری
            if (HttpContext.Session.GetString("SelectedGymId") != null)
            {
                return Guid.Parse(HttpContext.Session.GetString("SelectedGymId"));
            }

            // یا از URL
            if (Request.Query.ContainsKey("gymId"))
            {
                return Guid.Parse(Request.Query["gymId"]);
            }

            return Guid.Empty;
        }

        private bool IsGymCapacityAvailable(long trackingNumber)
        {
            // بررسی ظرفیت باشگاه
            // اینجا منطق بررسی ظرفیت را پیاده‌سازی کنید
            return true; // برای نمونه
        }

        private async Task ActivateUserMembership(Guid userId, Guid gymId)
        {
            // فعال‌سازی عضویت کاربر در باشگاه
            // اینجا منطق فعال‌سازی عضویت را پیاده‌سازی کنید
            await Task.CompletedTask;
        }

        private async Task<Payment> GetPaymentByTrackingNumber(long trackingNumber)
        {
            // دریافت پرداخت بر اساس شماره تراکنش
            // اینجا از دیتابیس دریافت کنید
            return await Task.FromResult<Payment>(null);
        }

        private async Task<List<Payment>> GetUserPayments(Guid userId)
        {
            // دریافت لیست پرداخت‌های کاربر
            // اینجا از دیتابیس دریافت کنید
            return await Task.FromResult(new List<Payment>());
        }

        #endregion
    }

    // مدل برای نمایش نتیجه
    public class PaymentResultViewModel
    {
        public Guid PaymentId { get; set; }
        public bool IsSuccessful { get; set; }
        public long TrackingNumber { get; set; }
        public decimal Amount { get; set; }
        public string Message { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

    public class ErrorViewModel
    {
        public string Message { get; set; }
    }
}
