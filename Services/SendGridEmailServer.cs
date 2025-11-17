using SendGrid;
using SendGrid.Helpers.Mail;

namespace NotatApp.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly string _apiKey;
        public SendGridEmailSender(IConfiguration cfg) => _apiKey = cfg["SendGrid:ApiKey"]!;
        public async Task SendAsync(string to, string subject, string body)
        {
            var client = new SendGridClient(_apiKey);
            var msg = MailHelper.CreateSingleEmail(
                new EmailAddress("no-reply@noteapp.local", "NoteApp"),
                new EmailAddress(to), subject, body,
                $"<p>{System.Net.WebUtility.HtmlEncode(body)}</p>");
            await client.SendEmailAsync(msg);
        }
    }

}
