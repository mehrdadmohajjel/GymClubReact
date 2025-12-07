namespace GymManager.Api.Models
{
    public class PaymentSettings
    {
        public MellatSettings Mellat { get; set; } = new();
    }

    public class MellatSettings
    {
        public long TerminalId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;
    }
}
