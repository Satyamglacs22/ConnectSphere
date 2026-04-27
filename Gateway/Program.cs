using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.WithProperty("Service", "Gateway")
    .CreateLogger();

builder.Host.UseSerilog();

// ── Rate Limiting ──────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(
    builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration,
    RateLimitConfiguration>();

// ── YARP Reverse Proxy ─────────────────────────────────────────────────────
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ── JWT Authentication ─────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── CORS ───────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",  // React
                "http://localhost:4200",  // Angular
                "http://localhost:5173"   // Vite
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ── Security Headers Middleware ────────────────────────────────────────────
app.Use(async (context, next) =>
{
    // Prevent clickjacking
    context.Response.Headers.Append(
        "X-Frame-Options", "DENY");

    // Prevent XSS attacks
    context.Response.Headers.Append(
        "X-Content-Type-Options", "nosniff");

    // XSS Protection
    context.Response.Headers.Append(
        "X-XSS-Protection", "1; mode=block");

    // Referrer Policy
    context.Response.Headers.Append(
        "Referrer-Policy", "no-referrer");

    // Content Security Policy
    context.Response.Headers.Append(
        "Content-Security-Policy",
        "default-src 'self'");

    await next();
});

// ── Rate Limiting ──────────────────────────────────────────────────────────
app.UseIpRateLimiting();

// ── Request Logging ────────────────────────────────────────────────────────
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} → {StatusCode} ({Elapsed:0.0000}ms)";
});

// ── CORS ───────────────────────────────────────────────────────────────────
app.UseCors("AllowFrontend");

// ── Auth ───────────────────────────────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

// ── Health Check ───────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    gateway = "YARP + Security",
    timestamp = DateTime.UtcNow,
    features = new[]
    {
        "Rate Limiting ✅",
        "Security Headers ✅",
        "CORS Policy ✅",
        "JWT Auth ✅",
        "Request Logging ✅"
    }
}));

// ── YARP Routes ────────────────────────────────────────────────────────────
app.MapReverseProxy();

app.Run();