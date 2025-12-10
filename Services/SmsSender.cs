using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace NotatApp.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly IAmazonSimpleNotificationService _sns;

        public SmsSender(IAmazonSimpleNotificationService sns)
        {
            _sns = sns;
        }

        public async Task SendAsync(string phoneNumber, string message)
        {
            var request = new PublishRequest
            {
                PhoneNumber = phoneNumber,
                Message = message
            };

            // if this throws, AuthController will catch via global exception handler / middleware
            await _sns.PublishAsync(request);
        }
    }
}