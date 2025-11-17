//using NotatApp.Models;
namespace NotatApp.Services;

public interface IEmailSender { Task SendAsync(string to, string subject, string body); }
