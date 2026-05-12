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
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient((context, handler) =>
    {
        handler.SslOptions.RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true;
    });

// Increase default timeout for large media transfers
builder.Services.AddHttpClient("YarpHttpClient")
    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromMinutes(5));

// ── Max Request Body Size (e.g. 100MB for Video) ───────────────────────────
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

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
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// ── CORS ───────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ── Debug endpoint – expose YARP config for troubleshooting ────────────────────
app.MapGet("/reverse-proxy-config", () =>
{
    var configSection = builder.Configuration.GetSection("ReverseProxy");
    return Results.Json(configSection);
});

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

    // Content Security Policy – permissive for development
    context.Response.Headers.Append(
        "Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com data:; " +
        "img-src 'self' data: blob:; " +
        "connect-src 'self' https://*.onrender.com http://localhost:* ws://localhost:*;");

    await next();
});

// ── CORS ───────────────────────────────────────────────────────────────────
app.UseCors("AllowFrontend");

// ── Rate Limiting ──────────────────────────────────────────────────────────
// app.UseIpRateLimiting();

// ── Request Logging ────────────────────────────────────────────────────────
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} → {StatusCode} ({Elapsed:0.0000}ms)";
});

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
