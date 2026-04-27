using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.WithProperty("Service", "Gateway") // change per service
    .CreateLogger();

builder.Host.UseSerilog();

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
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────────────────
app.UseSerilogRequestLogging();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// ── YARP Routes ────────────────────────────────────────────────────────────
app.MapReverseProxy();

// ── Health Check ───────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    gateway = "YARP",
    timestamp = DateTime.UtcNow,
    services = new[]
    {
        "Auth API    → http://localhost:5246",
        "Post API    → http://localhost:5247",
        "Like API    → http://localhost:5248",
        "Comment API → http://localhost:5249",
        "Follow API  → http://localhost:5250",
        "Notif API   → http://localhost:5251",
        "Feed API    → http://localhost:5252"
    }
}));

app.Run();