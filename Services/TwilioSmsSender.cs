using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace NotatApp.Services
{
public class TwilioSmsSender : ISmsSender
{
    private readonly string _sid, _token, _from;

    //this is in appsettings.json
    public TwilioSmsSender(IConfiguration cfg)
    {
        _sid   = cfg["Twilio:AccountSid"]!;
        _token = cfg["Twilio:AuthToken"]!;
        _from  = cfg["Twilio:FromNumber"]!;
        TwilioClient.Init(_sid, _token);
    }
    public async Task SendAsync(string to, string body)
    {
        await MessageResource.CreateAsync(
            to: new PhoneNumber(to),
            from: new PhoneNumber(_from),
            body: body);
    }
}
}
