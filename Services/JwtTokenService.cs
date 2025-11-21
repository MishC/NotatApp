using System.IdentityModel.Tokens.Jwt;  //JwtSecurityToken, JwtSecurityTokenHandler //this have classes for token creation
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text; //Encoding
using Microsoft.IdentityModel.Tokens; //SignInCredentials, SymmetricSecurityKey
using NotatApp.Models;


public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id), 
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new("name", user.UserName ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15), 
            signingCredentials: creds //credentials of this app
        );

        return new JwtSecurityTokenHandler().WriteToken(token); //Create token in the right format
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
