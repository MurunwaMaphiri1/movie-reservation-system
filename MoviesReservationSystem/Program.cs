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
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(conf));
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
            RequireExpirationTime = false,
            ValidIssuer = Env.GetString("DATABASE_ISSUER"),
            ValidAudience = Env.GetString("DATABASE_AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
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
    
    // Browsing
    options.AddTokenBucketLimiter("browsing", opt =>
    {
        opt.TokenLimit = 30;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.TokensPerPeriod = 10;
        opt.AutoReplenishment = true;
    });
    
    // Booking
    options.AddTokenBucketLimiter("booking", opt =>
    {
        opt.TokenLimit = 5;
        opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        opt.TokensPerPeriod = 2;
        opt.AutoReplenishment = true;
    });
    
    // Auth
    options.AddTokenBucketLimiter("auth", opt =>
    {
        opt.TokenLimit = 5;
        opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        opt.TokensPerPeriod = 1;
        opt.AutoReplenishment = true;
    });
    
    //Search Movie
    options.AddTokenBucketLimiter("search", opt =>
    {
        opt.TokenLimit = 20;
        opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        opt.TokensPerPeriod = 10;
        opt.AutoReplenishment = true;
    });
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

app.UseAuthorization();

app.MapControllers();

app.Run();