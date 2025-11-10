using Microsoft.AspNetCore.Identity.UI.Services;

namespace ScannerUtility
{
    public class EmailSender : IEmailSender
    {
        // Implemnt Email and Logic
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.CompletedTask;
        }
    }
}
