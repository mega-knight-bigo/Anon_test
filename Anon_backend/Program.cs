using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi.Models;
using FullstackTemplate.Infrastructure.Data;
using FullstackTemplate.Infrastructure.Repositories;
using FullstackTemplate.Infrastructure.SchemaFetching;
using FullstackTemplate.Infrastructure.ConnectionTesting;
using FullstackTemplate.Domain.Interfaces;
using FullstackTemplate.Application.Interfaces;
using FullstackTemplate.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositories ────────────────────────────────────────────
builder.Services.AddScoped<IConnectionRepository, ConnectionRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<IConnectionMetadataRepository, ConnectionMetadataRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ── Application Services ────────────────────────────────────
builder.Services.AddScoped<IConnectionService, ConnectionService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISchemaFetcherFactory, SchemaFetcherFactory>();
builder.Services.AddScoped<IConnectionTesterFactory, ConnectionTesterFactory>();

// ── HttpClient (for Auth0 userinfo calls) ───────────────────
builder.Services.AddHttpClient();

// ── Authentication (Auth0) ──────────────────────────────────
var auth0 = builder.Configuration.GetSection("Auth0");
var auth0Domain = auth0["Domain"]!;
var auth0Audience = auth0["Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = $"https://{auth0Domain}/";
    
    // Token validation parameters
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = "permissions",
        // Accept both the custom API audience AND the Auth0 userinfo audience
        // This handles cases where Auth0 API isn't fully configured
        ValidAudiences = new[] { auth0Audience, $"https://{auth0Domain}/userinfo" },
        ValidateAudience = true,
    };
    
    // Add logging for authentication events (helps with debugging)
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("Auth0");
            logger.LogWarning("Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("Auth0");
            var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}");
            logger.LogInformation("Token validated. Claims:\n  {Claims}", string.Join("\n  ", claims ?? Array.Empty<string>()));
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("Auth0");
            logger.LogWarning("Auth challenge: {Error} - {ErrorDescription}", 
                context.Error ?? "none", context.ErrorDescription ?? "none");
            return Task.CompletedTask;
        }
    };
});

// ── Authorization Policies (Auth0 RBAC) ─────────────────────
builder.Services.AddAuthorization(options =>
{
    // Permission-based policies - match Auth0 API permissions
    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireClaim("permissions", "manage:users"));
    
    options.AddPolicy("CanManageConnections", policy =>
        policy.RequireClaim("permissions", "manage:connections"));
    
    options.AddPolicy("CanManageConfigurations", policy =>
        policy.RequireClaim("permissions", "manage:configurations"));
    
    options.AddPolicy("CanManageJobs", policy =>
        policy.RequireClaim("permissions", "manage:jobs"));
    
    options.AddPolicy("CanReadActivity", policy =>
        policy.RequireClaim("permissions", "read:activity"));
    
    options.AddPolicy("CanReadDashboard", policy =>
        policy.RequireClaim("permissions", "read:dashboard"));
});

// ── Controllers & JSON ──────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ── Swagger ─────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Data Anonymization API", Version = "v1" });
});

// ── CORS (Secure Configuration) ─────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("Security:AllowedOrigins").Get<string[]>() 
    ?? new[] { "https://localhost:5000" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With")
            .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// ── Rate Limiting ───────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // Global rate limit: 100 requests per minute per IP
    options.AddPolicy("GlobalLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5
            }));
    
    // Strict rate limit for auth endpoints: 10 requests per minute
    options.AddPolicy("AuthLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }));
    
    // Sliding window for API calls: 200 requests per minute
    options.AddPolicy("ApiLimit", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));
});

var app = builder.Build();

// ── Origin Validation Middleware (CSRF Defense in Depth) ────
var configuredOrigins = builder.Configuration.GetSection("Security:AllowedOrigins").Get<string[]>() 
    ?? new[] { "https://localhost:5000" };

app.Use(async (context, next) =>
{
    // Skip origin check for safe methods and health checks
    var method = context.Request.Method;
    var path = context.Request.Path.Value ?? "";
    
    if (method == "GET" || method == "HEAD" || method == "OPTIONS" ||
        path.StartsWith("/api/health") || path.StartsWith("/swagger"))
    {
        await next();
        return;
    }
    
    // For state-changing requests, validate Origin header
    var origin = context.Request.Headers["Origin"].FirstOrDefault();
    var referer = context.Request.Headers["Referer"].FirstOrDefault();
    
    // If no origin (same-origin request or non-browser client), check referer or allow
    if (string.IsNullOrEmpty(origin))
    {
        // If referer is present, validate it
        if (!string.IsNullOrEmpty(referer))
        {
            var refererUri = new Uri(referer);
            var refererOrigin = $"{refererUri.Scheme}://{refererUri.Host}:{refererUri.Port}";
            if (!configuredOrigins.Contains(refererOrigin) && !configuredOrigins.Any(o => refererOrigin.StartsWith(o.TrimEnd('/'))))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: Invalid request origin");
                return;
            }
        }
        // No origin or referer = likely API client, let auth handle it
        await next();
        return;
    }
    
    // Validate origin against allowed list
    if (!configuredOrigins.Contains(origin))
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync("Forbidden: Invalid request origin");
        return;
    }
    
    await next();
});

// ── Security Headers Middleware ─────────────────────────────
app.Use(async (context, next) =>
{
    // Prevent MIME type sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    
    // Prevent clickjacking
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    
    // XSS protection (legacy browsers)
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    
    // Control referrer information
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // Permissions policy - restrict browser features
    context.Response.Headers.Append("Permissions-Policy", 
        "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
    
    // Content Security Policy (adjust for your needs)
    if (!context.Request.Path.StartsWithSegments("/swagger"))
    {
        context.Response.Headers.Append("Content-Security-Policy", 
            "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self'; connect-src 'self' https://*.auth0.com");
    }
    
    await next();
});

// ── Pipeline ────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // HSTS - only in production
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("ApiLimit");

// Serve SPA fallback for non-API routes
app.MapFallbackToFile("index.html");

// Auto-migrate in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
