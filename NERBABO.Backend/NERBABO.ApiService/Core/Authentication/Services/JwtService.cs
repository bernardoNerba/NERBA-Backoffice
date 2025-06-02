using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Authentication.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NerbaApp.Api.Services.AccountServices;

/// <summary>
/// Service for handling JWT (JSON Web Token) creation and management.
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _jwtKey;
    private readonly UserManager<User> _userManager;

    /// <summary>
    /// Initializes a new instance of the JwtService.
    /// </summary>
    /// Required configuration keys:
    /// - JWT:Key : Secret key for signing tokens
    /// - JWT:ExpiresInDays: Token expiration in days (default: 7)
    /// - JWT: Issuer : Token issuer claim
    /// <param name="config">Application configuration containg JWT settings
    /// set on appsettings.json</param>
    /// <exception cref="ArgumentNullException"></exception>
    public JwtService(IConfiguration config,
    UserManager<User> userManager)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _jwtKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JWT:Key"]
            ?? throw new ArgumentNullException("JWT:Key is not configured"))
        );
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

    }

    /// <summary>
    /// Creates a JWT Token for the specified user with standard claims.
    /// </summary>
    /// <param name="user">The user entity containing user data for claims</param>
    /// <returns>Encoded JWT token as string</returns>
    /// <exception cref="ArgumentNullException">Thrown if user parameter is null</exception>
    /// <remarks>
    /// The generated token includes these standard claims:
    /// - NameIdentifier (user ID)
    /// - Email
    /// - GivenName (first name)
    /// - Surname (last name)
    /// </remarks>
    public async Task<string> CreateJwt(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // creates standard claims for the token
        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, user.Id),
            new (ClaimTypes.Email, user.Email ?? ""),
            new (ClaimTypes.GivenName, user.Person?.FirstName ?? ""),
            new (ClaimTypes.Surname, user.Person ?.LastName ?? "")
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // configure signing credentials using the symmetric security key
        var credentials = new SigningCredentials(
            _jwtKey, SecurityAlgorithms.HmacSha256Signature);

        // set token properties
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(int.Parse(_config["JWT:ExpiresInDays"] ?? "7")),
            SigningCredentials = credentials,
            Issuer = _config["JWT:Issuer"]
        };

        // generate and return the token
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(jwt);
    }
}