using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace hamalba.Services
{
    public class FakeEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {

            Console.WriteLine($"Email to: {email}\nSubject: {subject}\nBody:\n{htmlMessage}");
            return Task.CompletedTask;
        }
    }
}