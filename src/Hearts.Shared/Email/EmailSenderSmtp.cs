using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Hearts.Shared.Email;

public class EmailSenderSmtp(IConfiguration configuration) : IEmailSender
{
    public Task SendEmailAsync(string recipient, string subject, string message)
    {
        SmtpClient smtpClient = new(configuration["Email:SmtpServer"])
        {
            Port = 587,
            Credentials = new NetworkCredential(configuration["Email:Username"], configuration["Email:Password"]),
            EnableSsl = true
        };

        MailMessage mailMessage = new()
        {
            From = new MailAddress(configuration["Email:From"]!),
            Subject = subject,
            Body = message,
            IsBodyHtml = false
        };
        mailMessage.To.Add(recipient);

        smtpClient.Send(mailMessage);

        return Task.CompletedTask;
    }
}
