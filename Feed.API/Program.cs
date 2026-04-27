using System.Text;
using Feed.API.Cache;
using Feed.API.Consumers;
using Feed.API.Data;
using Feed.API.HttpClients;
using Feed.API.Repositories;
using Feed.API.Repositories.Interfaces;
using Feed.API.Services;
using Feed.API.Services.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.WithProperty("Service", "Feed.API") // change per service
    .CreateLogger();

builder.Host.UseSerilog();

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<FeedDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration
        .GetConnectionString("Default")));

// ── Redis ──────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connStr = builder.Configuration["Redis:ConnectionString"]!;
    return ConnectionMultiplexer.Connect(connStr);
});

builder.Services.AddSingleton<FeedCacheService>();

// ── MassTransit + RabbitMQ ─────────────────────────────────────────────────
builder.Services.AddMassTransit(cfg =>
{
    // Register the consumer
    cfg.AddConsumer<PostCreatedConsumer>();

    cfg.UsingRabbitMq((ctx, rabbitCfg) =>
    {
        rabbitCfg.Host(
            builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        // Configure consumer endpoint
        rabbitCfg.ReceiveEndpoint("feed-post-created", e =>
        {
            e.ConfigureConsumer<PostCreatedConsumer>(ctx);
        });
    });
});

// ── Typed HTTP Clients with Polly ──────────────────────────────────────────
builder.Services.AddHttpClient<PostServiceClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:PostAPI"]!);
})
.AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3, attempt =>
        TimeSpan.FromSeconds(Math.Pow(2, attempt))));

builder.Services.AddHttpClient<FollowServiceClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:FollowAPI"]!);
})
.AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3, attempt =>
        TimeSpan.FromSeconds(Math.Pow(2, attempt))));

// ── Repositories & Services ────────────────────────────────────────────────
builder.Services.AddScoped<IFeedRepository, FeedRepository>();
builder.Services.AddScoped<IFeedService, FeedService>();

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

// ── Swagger ────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Feed API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── Health Checks ──────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("Default")!)
    .AddRedis(builder.Configuration["Redis:ConnectionString"]!);

// ── Controllers ────────────────────────────────────────────────────────────
builder.Services.AddControllers();

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();