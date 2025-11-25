using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System.Net;

namespace NotatApp.Services
{
    public class SesEmailSender : IEmailSender
    {
        private readonly IAmazonSimpleEmailService _ses;
        private readonly string _fromAddress;
        private readonly ILogger<SesEmailSender> _logger;

        public SesEmailSender(
            IAmazonSimpleEmailService ses,
            IConfiguration cfg,
            ILogger<SesEmailSender> logger)
        {
            _ses = ses;
            _logger = logger;
            _fromAddress = cfg["Email:From"] ?? "noreply@noteappsolutions.com";
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var request = new SendEmailRequest
            {
                Source = _fromAddress,
                Destination = new Destination { ToAddresses = new List<string> { to } },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Text = new Content(body),
                        Html = new Content($"<p>{WebUtility.HtmlEncode(body)}</p>")
                    }
                }
            };

            try
            {
                var response = await _ses.SendEmailAsync(request);
                _logger.LogInformation("SES email sent. MessageId={MessageId}, StatusCode={StatusCode}",
                    response.MessageId, response.HttpStatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via SES to {To}", to);
                throw;
            }
        }
    }

}
