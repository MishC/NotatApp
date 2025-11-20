using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotatApp.Models;
using NotatApp.Services;

namespace NotatApp.Controllers {

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;  //service has User Manager
    }

    [HttpGet("email")]
    public async Task<IActionResult> GetEmail()
    {
        var user = await _userService.GetCurrentUserAsync(User);
        if (user is null)
            return Unauthorized();

        return Ok(new { email = user.Email });
    }


    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userService.GetCurrentUserAsync(User);
        if (user is null)
            return Unauthorized();

        return Ok(new
        {
            userName = user.UserName,
            email = user.Email,
            phoneNumber = user.PhoneNumber,
            id = user.Id
        });
    }
    [HttpGet("username")]
    public async Task<IActionResult> GetUserName()
    {
        var user = await _userService.GetCurrentUserAsync(User);
        if (user is null)
            return Unauthorized();

        return Ok(new { userName = user.UserName });
    }

}

}
