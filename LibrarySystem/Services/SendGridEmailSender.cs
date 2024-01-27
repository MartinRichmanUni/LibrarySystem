using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LibrarySystem;

public class SendGridEmailSender : IEmailSender
{

    /**  
        Email Sender created with the help of https://learn.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-8.0&tabs=netcore-cli
        and https://www.ryadel.com/en/asp-net-core-send-email-messages-sendgrid-api/
        making use of SendGrid's API
    **/
    public SendGridEmailSender(IOptions<SendGridEmailSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }
    
    public SendGridEmailSenderOptions Options {get; set;}

    public async Task SendEmailAsync(string toEmail, string subject, string message )
    {
        await Execute(Options.SendGridKey, subject, message, toEmail);
    }

    private async Task<Response> Execute(string apiKey, string subject, string message, string toEmail)
    {
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(Options.SenderEmail, Options.SenderName),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        // Disable tracking data
        msg.SetClickTracking(false, false);
        msg.SetOpenTracking(false);
        msg.SetGoogleAnalytics(false);
        msg.SetSubscriptionTracking(false);

        return await client.SendEmailAsync(msg);
    }
}