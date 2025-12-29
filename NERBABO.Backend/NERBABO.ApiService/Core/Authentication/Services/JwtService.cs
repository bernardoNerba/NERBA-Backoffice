using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Authentication.Dtos;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Exceptions;
using NERBABO.ApiService.Shared.Models;
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
    private readonly ITokenBlacklistService _tokenBlacklistService;

    public JwtService(
        IConfiguration config,
        UserManager<User> userManager,
        AppDbContext context,
        ILogger<JwtService> logger,
        SignInManager<User> signInManager,
        ITokenBlacklistService tokenBlacklistService)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _jwtKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JWT:Key"]
            ?? throw new("JWT:Key is not configured"))
        );
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _context = context;
        _logger = logger;
        _signInManager = signInManager;
        _tokenBlacklistService = tokenBlacklistService ?? throw new ArgumentNullException(nameof(tokenBlacklistService));
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

        // update LastLogin
        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

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

    public async Task<bool> IsCoordOrAdminOfActionAsync(long actionId, string userId)
    {
        var action = await _context.Actions.FindAsync(actionId)
            ?? throw new ObjectNullException("Ação não encontrada.");

        // check if the user is the action coordenator
        if (action.CoordenatorId == userId)
            return true;

        // since is not coordenator check if is Admin
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new ObjectNullException("Utilizador não encontrado.");

        return await _userManager.IsInRoleAsync(user, "Admin");
    }

    public async Task<bool> IsCoordOrAdminOfActionViaMTAsync(long moduleTeachingId, string userId)
    {
        var mt = await _context.ModuleTeachings
            .Include(mt => mt.Action)
            .FirstOrDefaultAsync(mt => mt.Id == moduleTeachingId)
            ?? throw new ObjectNullException("ModuleTeaching não encontrado.");

        // check if the user is the action coordenator
        if (mt.Action.CoordenatorId == userId)
            return true;

        // since is not coordenator check if is Admin
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new ObjectNullException("Utilizador não encontrado.");

        return await _userManager.IsInRoleAsync(user, "Admin");
    }

    public async Task<bool> IsCoordOrAdminOfActionViaSessionAsync(long sessionId, string userId)
    {
        var session = await _context.Sessions
            .Include(s => s.ModuleTeaching).ThenInclude(mt => mt.Action)
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new ObjectNullException("Sessão não encontrada.");

        // check if the user is the action coordenator
        if (session.ModuleTeaching.Action.CoordenatorId == userId)
            return true;

        // since is not coordenator check if is Admin
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new ObjectNullException("Utilizador não encontrado.");

        return await _userManager.IsInRoleAsync(user, "Admin");
    }

    public async Task<bool> IsCoordOrAdminOfActionViaEnrollmentAsync(long enrollmentId, string userId)
    {
        var enrollment = await _context.ActionEnrollments
            .Include(ae => ae.Action)
            .FirstOrDefaultAsync(ae => ae.Id == enrollmentId)
            ?? throw new ObjectNullException("Sessão não encontrada.");

        // check if the user is the action coordenator
        if (enrollment.Action.CoordenatorId == userId)
            return true;

        // since is not coordenator check if is Admin
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new ObjectNullException("Utilizador não encontrado.");

        return await _userManager.IsInRoleAsync(user, "Admin");
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

    public async Task<Result> InvalidateTokenAsync(string token)
    {
        try
        {
            // Decode the token to get expiration time
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var expirationTime = jwtToken.ValidTo;

            // Add token to blacklist
            await _tokenBlacklistService.BlacklistTokenAsync(token, expirationTime);

            _logger.LogInformation("Token successfully invalidated for logout");
            return Result.Ok("Sucesso", "Token invalidado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating token during logout");
            return Result.Fail("Erro", "Erro ao invalidar token.");
        }
    }
}