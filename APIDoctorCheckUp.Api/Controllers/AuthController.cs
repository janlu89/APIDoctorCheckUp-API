using APIDoctorCheckUp.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace APIDoctorCheckUp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticates the admin user and returns a JWT access token.
    /// There is no user registration — credentials are set via environment variables.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        var expectedUsername = _configuration["Admin:Username"];
        var expectedPassword = _configuration["Admin:Password"];

        if (dto.Username != expectedUsername || dto.Password != expectedPassword)
            return Unauthorized(new { message = "Invalid credentials." });

        var token = GenerateToken();
        return Ok(token);
    }

    private TokenDto GenerateToken()
    {
        var secret   = _configuration["JWT:Secret"]!;
        var issuer   = _configuration["JWT:Issuer"]!;
        var audience = _configuration["JWT:Audience"]!;
        var expiry   = int.Parse(_configuration["JWT:ExpiryHours"] ?? "12");

        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt   = DateTime.UtcNow.AddHours(expiry);

        var token = new JwtSecurityToken(
            issuer:   issuer,
            audience: audience,
            claims:   [new Claim(ClaimTypes.Name, _configuration["Admin:Username"]!)],
            expires:  expiresAt,
            signingCredentials: credentials);

        return new TokenDto(
            AccessToken: new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt:   expiresAt);
    }
}
