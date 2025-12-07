namespace GymManager.Api.Services
{
    public class EmailService : IEmailService
    {
        public Task SendAsync(string email, string subject, string body)
        {
            Console.WriteLine("=== EMAIL MOCK ===");
            Console.WriteLine($"To: {email}");
            Console.WriteLine(subject);
            Console.WriteLine(body);
            return Task.CompletedTask;
        }
    }
}

