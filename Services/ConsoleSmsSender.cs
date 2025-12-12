namespace NotatApp.Services
{
public sealed class ConsoleSmsSender : ISmsSender
{
    private readonly ILogger<ConsoleSmsSender> _log;
    public ConsoleSmsSender(ILogger<ConsoleSmsSender> log) => _log = log;

    public Task SendAsync(string phoneNumber, string message)
    {
        _log.LogInformation("DEV SMS to {Phone}: {Msg}", phoneNumber, message);
        return Task.CompletedTask;
    }
}
}
