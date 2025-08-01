using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Account.Services;
using NERBABO.ApiService.Core.Actions.Cache;
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Actions.Services;
using NERBABO.ApiService.Core.Authentication.Models;
using NERBABO.ApiService.Core.Authentication.Services;
using NERBABO.ApiService.Core.Companies.Services;
using NERBABO.ApiService.Core.Courses.Cache;
using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Core.Courses.Services;
using NERBABO.ApiService.Core.Frames.Services;
using NERBABO.ApiService.Core.Global.Services;
using NERBABO.ApiService.Core.Modules.Cache;
using NERBABO.ApiService.Core.Modules.Services;
using NERBABO.ApiService.Core.People.Cache;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Core.People.Services;
using NERBABO.ApiService.Core.Students.Cache;
using NERBABO.ApiService.Core.Students.Models;
using NERBABO.ApiService.Core.Students.Services;
using NERBABO.ApiService.Core.Teachers.Cache;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Core.Teachers.Services;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Cache;
using NERBABO.ApiService.Shared.Middleware;
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
builder.Services.AddScoped<ITaxService, TaxService>();
builder.Services.AddScoped<IFrameService, FrameService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseActionService, CourseActionService>();

// Register Other services and middleware
builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
builder.Services.AddTransient<IResponseHandler, ResponseHandler>();
builder.Services.AddTransient<IAuthorizationHandler, ActiveUserHandler>();

// Regist Cache Services
builder.Services.AddScoped<ICacheKeyFabric<Course>, CacheKeyFabirc<Course>>();
builder.Services.AddScoped<ICacheCourseRepository, CacheCourseRepository>();
builder.Services.AddScoped<ICacheKeyFabric<NERBABO.ApiService.Core.Modules.Models.Module>, CacheKeyFabirc<NERBABO.ApiService.Core.Modules.Models.Module>>();
builder.Services.AddScoped<ICacheModuleRepository, CacheModuleRepository>();
builder.Services.AddScoped<ICacheKeyFabric<CourseAction>, CacheKeyFabirc<CourseAction>>();
builder.Services.AddScoped<ICacheActionRepository, CacheActionRepository>();
builder.Services.AddScoped<ICacheKeyFabric<Person>, CacheKeyFabirc<Person>>();
builder.Services.AddScoped<ICachePeopleRepository, CachePeopleRepository>();
builder.Services.AddScoped<ICacheKeyFabric<Teacher>, CacheKeyFabirc<Teacher>>();
builder.Services.AddScoped<ICacheTeacherRepository, CacheTeacherRepository>();
builder.Services.AddScoped<ICacheKeyFabric<Student>, CacheKeyFabirc<Student>>();
builder.Services.AddScoped<ICacheStudentsRepository, CacheStudentsRepository>();

// Connect to the database using Aspire injection from AppHost
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var connectionString = sp.GetRequiredService<IConfiguration>()
            .GetConnectionString("postgres");
    options.UseNpgsql(connectionString);
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

// Authorization Policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ActiveUser", policy =>
    policy.Requirements.Add(new ActiveUserRequirement()));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        var isDevelopment = builder.Environment.IsDevelopment();
        var allowedOrigins = DnsHelper.GenerateCorsOrigins(builder.Configuration, isDevelopment);
        
        if (isDevelopment)
        {
            // In development, be more permissive
            policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin)) return false;
                
                var uri = new Uri(origin);
                
                // Allow localhost with any port
                if (uri.Host == "localhost" || uri.Host == "127.0.0.1")
                    return true;
                
                // Allow local network IPs with Angular port
                var localIPs = DnsHelper.GetAllLocalIPAddresses();
                return localIPs.Contains(uri.Host) && (uri.Port == 4200 || uri.Port == 80 || uri.Port == 443);
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        }
        else
        {
            // In production, use strict origin checking
            if (allowedOrigins.Any())
            {
                policy.WithOrigins(allowedOrigins.ToArray());
            }
            else
            {
                // If no origins configured, allow same-network access
                policy.SetIsOriginAllowed(origin =>
                {
                    if (string.IsNullOrEmpty(origin)) return false;
                    
                    var uri = new Uri(origin);
                    var localIPs = DnsHelper.GetAllLocalIPAddresses();
                    
                    // Allow requests from same network
                    return localIPs.Any(ip => uri.Host.StartsWith(ip.Substring(0, ip.LastIndexOf('.'))));
                });
            }
            
            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
    
    // Add a more restrictive policy for production APIs
    options.AddPolicy("ProductionApi", policy =>
    {
        var configuredOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();
        if (configuredOrigins != null && configuredOrigins.Length > 0)
        {
            policy.WithOrigins(configuredOrigins)
                .WithHeaders("Content-Type", "Authorization")
                .WithMethods("GET", "POST", "PUT", "DELETE")
                .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

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
            .Select(x => x.ErrorMessage)
            .ToArray();

        var problemDetails = new ProblemDetails()
        {
            Title = "Erro de Validação",
            Status = StatusCodes.Status400BadRequest
        };

        problemDetails.Extensions["errors"] = errors;


        return new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    };
});



// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swaggerOptions =>
{
    swaggerOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                        Enter 'Bearer' [space] and then your token in the text input below.
                        Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    swaggerOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = $"Bearer"
                },
                Scheme = "Bearer",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    // Include XML comments for Swagger documentation
    swaggerOptions.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "NERBA API", 
        Version = "v1" 
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    swaggerOptions.IncludeXmlComments(xmlPath);
});

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

app.UseCors("AllowAngularApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NERBA API v1");
    });

    // Automatically open swagger UI in the default browser
    _ = Task.Run(async () =>
    {
        // Wait a short time for the server to be fully up and running.
        await Task.Delay(1500);
        // Get one of the server URLs (for example, the first one).
        var url = app.Urls.FirstOrDefault() ?? "http://localhost:8080";
        var swaggerUrl = $"{url}/swagger";
        try
        {
            Console.WriteLine($"Opening Swagger UI at {swaggerUrl}");
            Process.Start(new ProcessStartInfo
            {
                FileName = swaggerUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open Swagger UI automatically: {ex.Message}");
        }
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.MapControllers();

app.Run();
