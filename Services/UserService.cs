using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using NotatApp.Models;

namespace NotatApp.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager; //Dependency Injection for User, will have this cotructor
    }

    public Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        return _userManager.GetUserAsync(principal);
    }
}
