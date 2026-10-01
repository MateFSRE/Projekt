using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using CodeGames.Data;
using CodeGames.Middleware;
using CodeGames.Services;
using CodeGames.Services.Challenges;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    // Admin ConfigJson stane u ovo. Answer rute imaju strože ograničenje.
    options.Limits.MaxRequestBodySize = 256 * 1024;
});

// Connection string je u appsettings.Development.json ili u user-secrets, ne u Gitu.
var connectionString = builder.Configuration.GetConnectionString("CodeGamesDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Nedostaje connection string 'CodeGamesDb'. Dodaj ga u appsettings.Development.json ili u user-secrets.");
}

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwtSettings.Secret) || jwtSettings.Secret.Length < 32)
{
    throw new InvalidOperationException(
        "Nedostaje Jwt:Secret u user-secrets. Tajna mora imati najmanje 32 znaka.");
}

builder.Services.Configure<JwtSettings>(jwtSection);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<HealthService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DemoService>();
builder.Services.AddScoped<IChallengeHandler, PredictOutputHandler>();
builder.Services.AddScoped<IChallengeHandler, MultipleChoiceHandler>();
builder.Services.AddScoped<IChallengeHandler, FixBugHandler>();
builder.Services.AddScoped<IChallengeHandler, SwipeBugHandler>();
builder.Services.AddScoped<IChallengeHandler, FillBlanksHandler>();
builder.Services.AddScoped<IChallengeHandler, ReorderCodeHandler>();
builder.Services.AddScoped<IChallengeHandler, TraceExecutionHandler>();
builder.Services.AddScoped<IChallengeHandler, RobotMazeHandler>();
builder.Services.AddSingleton<GamificationService>();
builder.Services.AddScoped<ChallengesService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<AdminUserService>();
builder.Services.AddScoped<AdminChallengeService>();
builder.Services.AddScoped<AdminScoreService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = string.Join(
            " ",
            context.ModelState.Values
                .SelectMany(entry => entry.Errors)
                .Select(error => error.ErrorMessage)
                .Where(text => !string.IsNullOrWhiteSpace(text)));

        return new BadRequestObjectResult(new { message });
    };
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            RoleClaimType = "role",
            NameClaimType = "username",
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(AuthService.CookieName, out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { message = "Niste prijavljeni." });
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { message = "Nemate dozvolu za ovu radnju." });
            }
        };
    });

builder.Services.AddAuthorization();

var relaxedLimits = builder.Environment.IsEnvironment("Testing");
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "nepoznat",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = relaxedLimits ? 10_000 : 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.AddPolicy("demo", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "nepoznat",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = relaxedLimits ? 10_000 : 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        var policy = context.HttpContext.GetEndpoint()?.Metadata
            .GetMetadata<EnableRateLimitingAttribute>()?.PolicyName;
        var message = policy == "demo"
            ? "Previše demo pokušaja. Pričekaj minutu i pokušaj ponovno."
            : "Previše pokušaja prijave. Pričekaj minutu i pokušaj ponovno.";

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new { message }, cancellationToken);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
if (corsOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "Cors:Origins mora imati barem jedan origin, npr. http://localhost:5173.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .WithHeaders("Content-Type")
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await AdminSeeder.SeedAsync(app.Services);

app.Run();

public partial class Program;
