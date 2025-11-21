using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NotatApp.Models;
using NotatApp.Services;

namespace NotatApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _users;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        UserManager<User> users,
        IEmailSender emailSender,
        ISmsSender smsSender,
        IJwtTokenService jwtTokenService)
    {
        _users = users;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _jwtTokenService = jwtTokenService;
    }

    // Helper for creating of HttpOnly refresh cookie
    private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,                 // domene is HTTPS, takže OK
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        var res = await _users.CreateAsync(user, dto.Password);
        if (!res.Succeeded)
            return BadRequest(res.Errors);

        return Ok(new { message = "Registered" });
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _users.FindByEmailAsync(dto.Email);
        if (user is null)
            return BadRequest(new { message = "Wrong user name!" });

        if (!await _users.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Wrong password!" });

        var provider = dto.Channel?.ToLower() == "sms" ? "Phone" : "Email";

        var code = await _users.GenerateTwoFactorTokenAsync(user, provider);

        if (provider == "Email")
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest(new { message = "No email set" });

            await _emailSender.SendAsync(user.Email!, "Your login code", $"Your code: {code}");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                return BadRequest(new { message = "No phone set" });

            await _smsSender.SendAsync(user.PhoneNumber!, $"Your NoteApp login code: {code}");
        }

        return Ok(new { flowId = user.Id, message = "Code sent" });
    }

    // POST /api/auth/verify-2fa -> JWT + refresh token
    [HttpPost("verify-2fa")]
    public async Task<IActionResult> Verify2Fa([FromBody] Verify2FaDto dto)
    {
        var user = await _users.FindByIdAsync(dto.FlowId);
        if (user is null)
            return Unauthorized();

        var provider = dto.Channel?.ToLower() == "sms" ? "Phone" : "Email";

        var valid = await _users.VerifyTwoFactorTokenAsync(user, provider, dto.Code);
        if (!valid)
            return Unauthorized();

        // 1) access token
        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        // 2) refresh token
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshExpires = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshExpires;
        await _users.UpdateAsync(user);

        // 3) save refresh token do HttpOnly cookie
        SetRefreshTokenCookie(refreshToken, refreshExpires);

        // 4) access token in body
        return Ok(new { accessToken });
    }

    // POST /api/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            return Unauthorized();

        var user = await _users.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null || user.RefreshTokenExpiresAt == null)
            return Unauthorized();

        if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
            return Unauthorized(); 

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);

        // (generation of new refresh token)

        return Ok(new { accessToken = newAccessToken });
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // remove refresh token v DB (based on user id )
        if (User?.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (userId != null)
            {
                var user = await _users.FindByIdAsync(userId);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiresAt = null;
                    await _users.UpdateAsync(user);
                }
            }
        }

        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logged out" });
    }
}
