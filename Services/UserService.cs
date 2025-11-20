using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using NotatApp.Models;

namespace NotatApp.Services;

public class UserService : IUserService //Implementation of IUserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager; //Dependency Injection of UserManager<User>
    }

   public Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
{
    if (principal == null)
        throw new ArgumentNullException(nameof(principal));

    return _userManager.GetUserAsync(principal); //could be return async
}

}
