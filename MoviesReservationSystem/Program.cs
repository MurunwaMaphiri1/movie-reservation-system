using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MoviesReservationSystem;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using MoviesReservationSystem.Data;
using MoviesReservationSystem.Services.AuthService;
using MoviesReservationSystem.Services.EmailService;
using MoviesReservationSystem.Services.PasswordStrengthService;
using MoviesReservationSystem.Services.RedisService;
using StackExchange.Redis;
using NRedisStack;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

DotNetEnv.Env.Load();

var jwtKey = Env.GetString("JWT_SECRET_KEY");
StripeConfiguration.ApiKey = Env.GetString("STRIPE_SECRET_KEY");

var connectionString = $"Host={Env.GetString(("DATABASE_HOST"))};" +
                       $"Port={Env.GetInt(("DATABASE_PORT"))};" +
                       $"Database={Env.GetString(("DATABASE_NAME"))};" +
                       $"Username={Env.GetString(("DATABASE_USERNAME"))};" +
                       $"Password={Env.GetString(("DATABASE_PASSWORD"))};";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

ConfigurationOptions conf = new ConfigurationOptions
{
    EndPoints = { "redis:6379" }
};

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordStrengthService, PasswordStrengthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(conf));
builder.Services.AddSingleton<ISeatLockService, SeatLockService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = true,
            ValidIssuer = Env.GetString("DATABASE_ISSUER"),
            ValidAudience = Env.GetString("DATABASE_AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            // Tokens carry a bare "Role" claim, not the ClaimTypes.Role URI that IsInRole expects by default
            RoleClaimType = "Role"
        };
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, token) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds} seconds";

            ProblemDetailsFactory problemDetailsFactory = context.HttpContext.RequestServices
                .GetRequiredService<ProblemDetailsFactory>();
            Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = problemDetailsFactory
                .CreateProblemDetails(
                    context.HttpContext,
                    StatusCodes.Status429TooManyRequests,
                    "Too Many Requests",
                    detail: $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds."
                );
            await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
        }
    };
    
    // Each caller gets their own bucket, keyed on user id when authenticated and falling back to IP
    void AddPerCallerTokenBucket(string policyName, int tokenLimit, int tokensPerPeriod,
        TimeSpan replenishmentPeriod)
    {
        options.AddPolicy(policyName, httpContext =>
        {
            var accountId = httpContext.User.FindFirst("Id")?.Value;
            var caller = accountId != null
                ? $"{httpContext.User.FindFirst("AccountType")?.Value}:{accountId}"
                : httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

            return RateLimitPartition.GetTokenBucketLimiter($"{policyName}:{caller}",
                _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = tokenLimit,
                    ReplenishmentPeriod = replenishmentPeriod,
                    TokensPerPeriod = tokensPerPeriod,
                    AutoReplenishment = true
                });
        });
    }

    // Browsing
    AddPerCallerTokenBucket("browsing", tokenLimit: 30, tokensPerPeriod: 10, TimeSpan.FromSeconds(10));

    // Booking
    AddPerCallerTokenBucket("booking", tokenLimit: 5, tokensPerPeriod: 2, TimeSpan.FromMinutes(1));

    // Seat locking: the frontend sends one lock/unlock request per seat click, so this needs headroom
    AddPerCallerTokenBucket("seat-lock", tokenLimit: 30, tokensPerPeriod: 10, TimeSpan.FromSeconds(30));

    // Auth
    AddPerCallerTokenBucket("auth", tokenLimit: 5, tokensPerPeriod: 1, TimeSpan.FromMinutes(1));

    //Search Movie
    AddPerCallerTokenBucket("search", tokenLimit: 20, tokensPerPeriod: 10, TimeSpan.FromMinutes(1));
});

builder.Services.AddOptions();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policyBuilder =>
        {
            policyBuilder.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddAuthorization();
    

var app = builder.Build();

app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Must precede UseRateLimiter so limiter partitions can key on the authenticated user
app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();