using Scalar.AspNetCore;
using HomeInventoryManager.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using HomeInventoryManager.Api.Services.Interfaces;
using HomeInventoryManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();

//add db connection
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
var JWTSecret = Environment.GetEnvironmentVariable("JWT_SECRET");

var isTesting = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing";
if (!isTesting)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

builder.Services.AddOpenApi();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTSecret!)),
        };
    });

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserDeleteService, UserDeleteService>();
builder.Services.AddScoped<IItemAddService, ItemAddService>();

builder.Host.UseSerilog();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("register", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(2);
        limiterOptions.PermitLimit = 5; // 5 requests per minute per IP/user
    });
});

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();

public partial class Program { } //integration testing reference to generate testhost.deps.json in proper directory.
