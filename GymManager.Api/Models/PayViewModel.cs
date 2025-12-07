using System.ComponentModel.DataAnnotations;

namespace GymManager.Api.Models
{
    public class PayViewModel
    {
        [Required(ErrorMessage = "لطفا مبلغ را وارد کنید")]
        [Range(1000, 100000000, ErrorMessage = "مبلغ باید بین 1,000 تا 100,000,000 تومان باشد")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "لطفا درگاه پرداخت را انتخاب کنید")]
        public string SelectedGateway { get; set; } = "Melli"; // Melli, Parsian, Saman, etc.

        public bool GenerateTrackingNumberAutomatically { get; set; } = true;

        public long? TrackingNumber { get; set; }
    }

    public class RefundViewModel
    {
        [Required(ErrorMessage = "لطفا شماره تراکنش را وارد کنید")]
        public long TrackingNumber { get; set; }
    }
}
