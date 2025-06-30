using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Authentication.Dtos;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;
using OpenTelemetry.Trace;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NERBABO.ApiService.Core.Authentication.Services;

/// <summary>
/// Service for handling JWT (JSON Web Token) creation and management.
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _jwtKey;
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<JwtService> _logger;
    private readonly SignInManager<User> _signInManager;
    public JwtService(
        IConfiguration config,
        UserManager<User> userManager,
        AppDbContext context,
        ILogger<JwtService> logger,
        SignInManager<User> signInManager)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _jwtKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JWT:Key"]
            ?? throw new ("JWT:Key is not configured"))
        );
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _context = context;
        _logger = logger;
        _signInManager = signInManager;
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
    private async Task<string> CreateJwt(User user)
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

    public async Task<Result<LoggedInUserDto>> GenerateRefreshTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<LoggedInUserDto>
                .Fail("Não Autorizado.", "Token inválido.",
                StatusCodes.Status404NotFound);

        return Result<LoggedInUserDto>
            .Ok(await GetPersonAndBuildJwt(user));
    }

    public async Task<Result<LoggedInUserDto>> GenerateJwtOnLoginAsync(LoginDto model)
    {
        // Try to find user by username first, then fall back to email
        var user = await _userManager.FindByNameAsync(model.UsernameOrEmail)
            ?? await _userManager.FindByEmailAsync(model.UsernameOrEmail);
        if (user is null)
        {
            _logger.LogWarning("Login attempt failed for {UsernameOrEmail}. User not found.", model.UsernameOrEmail);
            return Result<LoggedInUserDto>
                .Fail("Erro de Validação", "Email/Username ou password inválidos.");
        }
        
        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt failed for {UsernameOrEmail}. User is blocked.", model.UsernameOrEmail);
            return Result<LoggedInUserDto>
                .Fail("Não Autorizado.", "Utilizador bloqueado.", 
                StatusCodes.Status401Unauthorized);
        }

        var validPassword = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!validPassword.Succeeded)
        {
            _logger.LogWarning("Login attempt failed for {UsernameOrEmail}. Inv�lid Password.", model.UsernameOrEmail);
            return Result<LoggedInUserDto>
                .Fail("Erro de Validao", "Email/Username ou password inv�lidos.");
        }

        var loggedInUser = await GetPersonAndBuildJwt(user);
        return Result<LoggedInUserDto>
            .Ok(loggedInUser);
    }


    private async Task<LoggedInUserDto> GetPersonAndBuildJwt(User user)
    {
        var person = await _context.People
            .FirstOrDefaultAsync(p => p.Id == user.PersonId);

        return new LoggedInUserDto(
            person?.FirstName ?? "",
            person?.LastName ?? "",
            await CreateJwt(user)
            );
    }
}