using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace NotatApp.Services
{
    public class ConsoleEmailSender : IEmailSender
    {
        private readonly ILogger<ConsoleEmailSender> _logger;

        public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string to, string subject, string body)
        {
            _logger.LogInformation(
                "DEV FAKE EMAIL: To={To}, Subject={Subject}, Body={Body}",
                to, subject, body
            );

            // nič reálne neposielame v DEV
            return Task.CompletedTask;
        }
    }
}
