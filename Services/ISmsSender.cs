
namespace NotatApp.Services;
public interface ISmsSender   { Task SendAsync(string to, string body); }
