using System.Security.Claims;
using NotatApp.Models;
namespace NotatApp.Services{

public interface IUserService
{
    Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal);
}
}