using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NerbaApp.Api.Services.AccountServices;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Account.Services;
using NERBABO.ApiService.Core.Authentication.Services;
using NERBABO.ApiService.Core.Global.Services;
using NERBABO.ApiService.Core.People.Services;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();

// Connect to Redis
var redisConnectionString = builder.Configuration.GetConnectionString("redis");

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect(redisConnectionString
    ?? throw new InvalidOperationException("Redis connection string is not configured."));
});


// Dependency Injection Container
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IPeopleService, PeopleService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IGeneralInfoService, GeneralInfoService>();
builder.Services.AddScoped<IIvaTaxService, IvaTaxService>();

// Connect to the database using Aspire injection from AppHost
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    try
    {
        var connectionString = sp.GetRequiredService<IConfiguration>()
                .GetConnectionString("postgres");
        options.UseNpgsql(connectionString);

    }
    catch (InvalidOperationException ex)
    {
        Debug.WriteLine($"Something went wrong with database connection:\n{ex}");
    }
});



// Configure Identity
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;

})
    .AddRoles<IdentityRole>()                    // be able to add roles
    .AddRoleManager<RoleManager<IdentityRole>>() // be able to make use of RoleManager (creating... roles)
    .AddEntityFrameworkStores<AppDbContext>()    // providing our context to the identity system
    .AddSignInManager<SignInManager<User>>()     // make use of sign in manager in order to sign in user
    .AddUserManager<UserManager<User>>()         // make use of user manager in order to create user
    .AddDefaultTokenProviders();                 // to be able to create tokens for email confirmation

// Authenticate user using JWT Token Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"] ?? "")
        ),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = false
    };
});

// Cors
builder.Services.AddCors();

// Add Controllers with Route Convention
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

// Error Configuration
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
        .Where(e => e.Value!.Errors.Count > 0)
        .SelectMany(e => e.Value!.Errors)
        .Select(x => x.ErrorMessage).ToArray();

        var toReturn = new
        {
            Errors = errors
        };

        return new BadRequestObjectResult(toReturn);
    };
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();



// Handle migrations and seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext = services.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    var seeder = new SeedDataHelp(userManager, roleManager, dbContext, builder.Configuration);


    await seeder.InitializeAsync();
}

app.UseCors(options =>
{
    options.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .AllowAnyOrigin()
        .WithOrigins(builder.Configuration["JWT:ClientUrl"] ?? "");
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
