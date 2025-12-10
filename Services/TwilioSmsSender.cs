using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
    
namespace NotatApp.Services
{
    public partial class TwilioSmsSender : ISmsSender
    {
        private const int MaxBodyLength = 1600;
        private readonly string _sid, _token, _from;

        // this is in appsettings.json
        public TwilioSmsSender(IConfiguration cfg)
        {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));

            _sid = cfg["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio:AccountSid is not configured.");
            _token = cfg["Twilio:AuthToken"] ?? throw new InvalidOperationException("Twilio:AuthToken is not configured.");
            _from = cfg["Twilio:FromNumber"] ?? throw new InvalidOperationException("Twilio:FromNumber is not configured.");

            if (!IsValidE164(_from))
                throw new FormatException("Twilio:FromNumber must be in E.164 format (e.g. +1234567890).");

            TwilioClient.Init(_sid, _token);
        }

        public async Task SendAsync(string to, string body)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Parameter cannot be null or whitespace.", nameof(to));
            if (!IsValidE164(to))
                throw new ArgumentException("Recipient phone number must be in E.164 format (e.g. +1234567890).", nameof(to));

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("Message body cannot be empty.", nameof(body));
            if (body.Length > MaxBodyLength)
                throw new ArgumentException($"Message body cannot exceed {MaxBodyLength} characters.", nameof(body));

            await MessageResource.CreateAsync(
                to: new PhoneNumber(to),
                from: new PhoneNumber(_from),
                body: body);
        }

        private static bool IsValidE164(string number)
        {
            if (string.IsNullOrEmpty(number)) return false;
            // E.164 format requires a leading '+' and up to 15 digits total
            return MyRegex().IsMatch(number);
        }

        [GeneratedRegex(@"^\+[1-9]\d{1,14}$")]
        private static partial Regex MyRegex();
    }
}
