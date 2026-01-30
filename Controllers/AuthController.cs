using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NotatApp.Models;
using NotatApp.Services;


namespace NotatApp.Controllers;


//Service+Controller here
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _users;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;
    private readonly IHostEnvironment _env;


    //constructor asks 
    public AuthController(                             //DI 
       UserManager<User> users,
       IEmailSender emailSender,
       ISmsSender smsSender,
       IJwtTokenService jwtTokenService,
       ILogger<AuthController> logger,
       IHostEnvironment env)    //ILogger from ASP.NET Core
    {
        _users = users;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _env = env;
    }

    // Helper for creating of HttpOnly refresh cookie
    private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

    }

    [HttpGet("health")]
    public IActionResult HealthCheck() => Ok("Auth service is running.");

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto? dto)
    {
        if (dto == null)
            return BadRequest(new { message = "Registration data is missing." });

        if (!ModelState.IsValid)
            return BadRequest(new { message = "The data provided is not valid." });

        var user = new User
        {
            UserName = dto.Email, // Identity usually uses Email as Username
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        try
        {
            var res = await _users.CreateAsync(user, dto.Password);

            if (!res.Succeeded)
            {
                // Check for specific Identity error codes
                if (res.Errors.Any(e => e.Code == "DuplicateUserName" || e.Code == "DuplicateEmail"))
                {
                    return Conflict(new { message = "This email address is already registered." });
                }

                // Fallback for other errors (e.g., Password too weak)
                var firstError = res.Errors.FirstOrDefault()?.Description ?? "Registration failed.";
                return BadRequest(new { message = firstError });
            }

            return Ok(new { message = "Account created successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration crash");
            return StatusCode(500, new { message = "A server error occurred. Please try again later." });
        }
    }

    // POST /api/auth/login
    [HttpPost("login")]
    [EnableRateLimiting("login")]

    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _users.FindByEmailAsync(dto.Email);
        if (user is null)
            return  Unauthorized(new { message = "Invalid email or password." });

        if (!await _users.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Wrong password!" });

        if (await _users.IsLockedOutAsync(user))
            return StatusCode(403, new { message = "Account is temporarily locked. Try again later." });

        if (!await _users.CheckPasswordAsync(user, dto.Password))
        {
            // Increment access failed count for lockout protection
            await _users.AccessFailedAsync(user);
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Reset failed count on successful password check
        await _users.ResetAccessFailedCountAsync(user);


        // Check if account is locked out (if you have lockout enabled)
        if (await _users.IsLockedOutAsync(user))
            return StatusCode(403, new { message = "Account is temporarily locked. Try again later." });

        var provider = dto.Channel?.ToLower() == "sms" ? "Phone" : "Email";
        var code = await _users.GenerateTwoFactorTokenAsync(user, provider);


        if (!_env.IsDevelopment())
        {
            if (provider == "Email")
            {
                if (string.IsNullOrWhiteSpace(user.Email))
                    return BadRequest(new { message = "No email set" });

                await _emailSender.SendAsync(
                    user.Email!,
                    "Your login code",
                    $"Your code: {code}"
                );

                return Ok(new { flowId = user.Id, message = "Code sent" });
            }

            // == SMS ==
            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                return BadRequest(new { message = "No phone set" });

            try
            {
                await _smsSender.SendAsync(
                    user.PhoneNumber!,
                    $"Your NoteApp login code: {code}"
                );

                return Ok(new { flowId = user.Id, message = "Code sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send SMS 2FA code to {Phone} for user {UserId}",
                    user.PhoneNumber, user.Id);

                return StatusCode(500, new { message = "Could not deliver verification code." });
            }
        }
        else
        {
            _logger.LogInformation("DEV LOGIN CODE for {Email}: {Code} via {Provider}", user.Email, code, provider);
            // Optionally: return { code } for local UI; keep it dev-only
            return Ok(new { flowId = user.Id, message = "Code sent (dev)", code });
        }
    }
    // POST /api/auth/verify-2fa -> JWT + refresh token
    [HttpPost("verify-2fa")]
    public async Task<IActionResult> Verify2Fa([FromBody] Verify2FaDto dto)
    {


        var user = await _users.FindByIdAsync(dto.FlowId);
        if (user is null)
            return Unauthorized();

        var provider = dto.Channel?.ToLower() == "sms" ? "Phone" : "Email";
        bool valid;

        if (!_env.IsDevelopment())
        {

            valid = await _users.VerifyTwoFactorTokenAsync(user, provider, dto.Code);
        }

        else { valid = dto.Code == "123456"; }
        if (!valid)
            return Unauthorized(new { message = "Invalid code." });

        // 1) access token generate
        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        // 2) refresh token generate
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshExpires = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshExpires;
        await _users.UpdateAsync(user); //inside db, new refresh token, everytime you generate access token

        // 3) save refresh token do HttpOnly cookie
        SetRefreshTokenCookie(refreshToken, refreshExpires);

        // 4) access token in body
        return Ok(new { accessToken });
    }



    // POST /api/auth/refresh !it checks when accesstoken is expired, 401
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

        // (generation of new  accesstoken, refresh token is not expired)


        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);


        return Ok(new { accessToken = newAccessToken }); //if 
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

