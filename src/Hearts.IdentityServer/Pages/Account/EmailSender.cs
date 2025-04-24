using Azure.Identity;
using Hearts.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using Microsoft.Identity.Client;

namespace Hearts.IdentityServer.Pages.Account;

public class EmailSender(IConfiguration configuration) : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email,
        string confirmationLink) => this.SendEmailAsync(email, "Confirm your email",
        "<html lang=\"en\"><head></head><body>Please confirm your account by " +
        $"<a href='{confirmationLink}'>clicking here</a>.</body></html>");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email,
        string resetLink) => this.SendEmailAsync(email, "Reset your password",
        "<html lang=\"en\"><head></head><body>Please reset your password by " +
        $"<a href='{resetLink}'>clicking here</a>.</body></html>");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email,
        string resetCode) => this.SendEmailAsync(email, "Reset your password",
        "<html lang=\"en\"><head></head><body>Please reset your password " +
        $"using the following code:<br>{resetCode}</body></html>");

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        string? tenantId = configuration["Email:TenantId"];
        string? clientId = configuration["Email:ClientId"];
        string? clientSecret = configuration["Email:ClientSecret"];
        string? from = configuration["Email:From"];

        IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
            .Build();

        ClientSecretCredential credential = new(tenantId, clientId, clientSecret);
        GraphServiceClient graphClient = new(credential);

        Message message = new()
        {
            From = new Recipient
            {
                EmailAddress = new EmailAddress
                {
                    Address = from
                }
            },
            Subject = subject,
            Body = new ItemBody { Content = body, ContentType = BodyType.Html },
            ToRecipients =
            [
                new Recipient { EmailAddress = new EmailAddress { Address = toEmail } }
            ]
        };

        SendMailPostRequestBody sendMailPostRequestBody = new()
        {
            Message = message,
            SaveToSentItems = true
        };

        await graphClient
            .Users[from]
            .SendMail
            .PostAsync(sendMailPostRequestBody);
    }
}
