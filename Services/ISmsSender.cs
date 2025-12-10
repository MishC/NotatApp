
namespace NotatApp.Services
{
    public interface ISmsSender
    {
        Task SendAsync(string phoneNumber, string message);
    }
}