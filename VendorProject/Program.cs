using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VendorProject.Common.Mappings;
using VendorProject.EF.Data;
using VendorProject.Infrastructure.Services.Auth;
using VendorProject.Infrastructure.Services.Sms;
using VendorProject.Services.Repositories;
using VendorProject.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<MarketplaceDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure authentication and authorization settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings not configured in appsettings.json");

var otpSettings = builder.Configuration.GetSection("OtpSettings").Get<OtpSettings>()
    ?? new OtpSettings();

// Register settings
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton(otpSettings);

// Register SMS providers
builder.Services.AddSingleton<ISmsSender>(sp => new MockSmsSender(sp.GetRequiredService<ILogger<MockSmsSender>>()));

// Optional: Add real providers based on configuration
var twilioSettings = builder.Configuration.GetSection("SmsProviders:Twilio").Get<TwilioSettings>();
if (twilioSettings != null && !string.IsNullOrEmpty(twilioSettings.AccountSid))
{
    builder.Services.AddSingleton<ISmsSender>(sp => new TwilioSmsSender(sp.GetRequiredService<ILogger<TwilioSmsSender>>(), twilioSettings));
}

var vonageSettings = builder.Configuration.GetSection("SmsProviders:Vonage").Get<VonageSettings>();
if (vonageSettings != null && !string.IsNullOrEmpty(vonageSettings.ApiKey))
{
    builder.Services.AddSingleton<ISmsSender>(sp => new VonageSmsSender(sp.GetRequiredService<ILogger<VonageSmsSender>>(), vonageSettings));
}

var awsSnsSettings = builder.Configuration.GetSection("SmsProviders:AwsSns").Get<AwsSnsSettings>();
if (awsSnsSettings != null && !string.IsNullOrEmpty(awsSnsSettings.AccessKey))
{
    builder.Services.AddSingleton<ISmsSender>(sp => new AwsSnsSmsSender(sp.GetRequiredService<ILogger<AwsSnsSmsSender>>(), awsSnsSettings));
}

// Register SMS provider factory
builder.Services.AddSingleton<ISmsProviderFactory, SmsProviderFactory>();

// Register authentication services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Configure JWT Authentication
var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Log JWT validation failures
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Authentication failed: {Message}", context.Exception?.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogDebug("Token validated for user {Subject}", context.Principal?.FindFirst("sub")?.Value);
            return Task.CompletedTask;
        }
    };
});

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireVendor", policy =>
        policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Vendor"));

    options.AddPolicy("RequireBuyer", policy =>
        policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Buyer"));

    options.AddPolicy("RequireTransporter", policy =>
        policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Transporter"));

    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin"));

    options.AddPolicy("PhoneVerified", policy =>
        policy.RequireClaim("PhoneVerified", "True"));

    options.AddPolicy("KycVerified", policy =>
        policy.RequireClaim("KycVerified", "True"));
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Register Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IVendorListingRepository, VendorListingRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransportRouteRepository, TransportRouteRepository>();

// Register Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IVendorListingService, VendorListingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITransportRouteService, TransportRouteService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
