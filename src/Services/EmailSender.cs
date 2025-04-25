using hamalba.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var client = new SmtpClient("smtp.gmail.com", 587)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.AppPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.Email, "HamalBa Pomočnik"),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true,
        };

        mailMessage.To.Add(email);

        return client.SendMailAsync(mailMessage);
    }
}
