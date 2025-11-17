using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using NotatApp.Models;
using NotatApp.Services;

namespace NotatApp.Controllers;

[ApiController]
[Route("api/[controller]")] // => /api/auth/...
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
    // 1. check password
    // 2. send OTP via SMS (Twilio) or Email
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _users.FindByEmailAsync(dto.Email);
        if (user is null || !await _users.CheckPasswordAsync(user, dto.Password))
            return Unauthorized();

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

    // POST /api/auth/verify-2fa
    // -> generate JWT, if Twilio code is ok
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

        // - OTP is valid
        // => issue JWT
        var token = _jwtTokenService.GenerateToken(user);

        return Ok(new { accessToken = token });
    }
}
