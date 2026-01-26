# Developer Reference Card - OTP Authentication

**Quick Reference for Developers**
Last Updated: January 26, 2026

---

## 🚀 Quick Setup (5 Minutes)

```bash
# 1. Build solution
cd c:\AppDevelopment
dotnet build

# 2. Apply database migration
dotnet ef database update -p VendorProject.EF -s VendorProject

# 3. Run application
cd VendorProject
dotnet run

# 4. Open Swagger UI
# https://localhost:7001/swagger/ui.html
```

---

## 📋 API Endpoints Quick Reference

### Registration
```http
POST /api/auth/register
Content-Type: application/json

{
  "phoneNumber": "+919876543210",
  "fullName": "John Doe",
  "email": "john@example.com",
  "desiredRole": "Buyer"
}
```

### Login
```http
POST /api/auth/send-login-otp
Content-Type: application/json

{
  "phoneNumber": "+919876543210"
}
```

### Verify & Get Token
```http
POST /api/auth/verify-login
Content-Type: application/json

{
  "phoneNumber": "+919876543210",
  "otpCode": "123456"
}

Response:
{
  "accessToken": "eyJ...",
  "refreshToken": "ABC...",
  "expiresIn": 900,
  "tokenType": "Bearer"
}
```

### Using Token
```http
GET /api/auth/profile
Authorization: Bearer eyJ...
```

### Refresh Token
```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "ABC..."
}
```

### Logout
```http
POST /api/auth/logout
Authorization: Bearer eyJ...
```

---

## 🔐 JWT Claims Reference

### Claims in Access Token
```json
{
  "iss": "VendorProject",
  "aud": "VendorProjectUsers",
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "name": "John Doe",
  "email": "john@example.com",
  "Phone": "+919876543210",
  "PhoneVerified": "True",
  "KycVerified": "False",
  "role": ["Buyer"],
  "exp": 1706251800
}
```

### Extracting Claims in C#
```csharp
// In Controller
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var phone = User.FindFirst("Phone")?.Value;
var isPhoneVerified = bool.Parse(User.FindFirst("PhoneVerified")?.Value ?? "false");
var roles = User.FindAll(ClaimTypes.Role);
```

---

## 🛡️ Authorization Policies

### Apply to Controller Methods
```csharp
[Authorize(Policy = "RequireVendor")]
public IActionResult CreateListing() { ... }

[Authorize(Policy = "PhoneVerified")]
public IActionResult BuyProduct() { ... }

[Authorize(Policy = "KycVerified")]
public IActionResult SubmitDocuments() { ... }

[Authorize]
[Authorize(Roles = "Admin")]
public IActionResult AdminOnly() { ... }
```

### Available Policies
- `"RequireVendor"` - Vendor role required
- `"RequireBuyer"` - Buyer role required
- `"RequireTransporter"` - Transporter role required
- `"RequireAdmin"` - Admin role required
- `"PhoneVerified"` - Phone must be verified
- `"KycVerified"` - KYC must be verified

---

## ⚙️ Configuration Reference

### appsettings.json Keys

**JWT Settings**
```json
"JwtSettings": {
  "Secret": "min-32-chars-change-in-production",
  "Issuer": "VendorProject",
  "Audience": "VendorProjectUsers",
  "AccessTokenExpirationMinutes": 15,
  "RefreshTokenExpirationDays": 7
}
```

**OTP Settings**
```json
"OtpSettings": {
  "OtpLength": 6,              // 6-digit code
  "OtpValidityMinutes": 10,    // 10 minute expiry
  "MaxAttempts": 3,            // 3 wrong attempts
  "RateLimitRequestsPerMinute": 3
}
```

**SMS Providers**
```json
"SmsProviders": {
  "Twilio": {
    "AccountSid": "your-sid",
    "AuthToken": "your-token",
    "FromNumber": "+1234567890"
  }
}
```

---

## 🔌 Dependency Injection

### Injecting Services in Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IOtpService _otpService;
    private readonly IJwtTokenService _tokenService;

    public MyController(
        IAuthenticationService authService,
        IOtpService otpService,
        IJwtTokenService tokenService)
    {
        _authService = authService;
        _otpService = otpService;
        _tokenService = tokenService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetData(CancellationToken ct)
    {
        var user = await _authService.GetUserAsync(userId, ct);
        return Ok(user);
    }
}
```

### Registering Custom Service
```csharp
// In Program.cs
builder.Services.AddScoped<IMyService, MyService>();

// Custom SMS Provider
var customSettings = builder.Configuration.GetSection("SmsProviders:Custom").Get<CustomSettings>();
if (customSettings?.IsConfigured == true)
{
    builder.Services.AddSingleton<ISmsSender>(sp => 
        new CustomSmsSender(sp.GetRequiredService<ILogger<CustomSmsSender>>(), customSettings));
}
```

---

## 🧪 Testing Examples

### Test OTP Generation
```csharp
[Test]
public async Task GenerateOtp_Should_CreateValidOtp()
{
    // Arrange
    var phoneNumber = "+919876543210";
    var otpService = new OtpService(_dbContext, _providerFactory, _logger, _settings);

    // Act
    var (success, message, otpCode) = await otpService.GenerateAndSendOtpAsync(
        phoneNumber, 
        OtpPurpose.Login);

    // Assert
    Assert.IsTrue(success);
    Assert.IsNotNullOrEmpty(otpCode);
    Assert.AreEqual(6, otpCode.Length);
}
```

### Test JWT Token Generation
```csharp
[Test]
public void GenerateAccessToken_Should_CreateValidJwt()
{
    // Arrange
    var user = new User
    {
        Id = Guid.NewGuid(),
        FullName = "Test User",
        Phone = "+919876543210",
        IsPhoneVerified = true
    };
    var tokenService = new JwtTokenService(_dbContext, _logger, _jwtSettings);

    // Act
    var token = tokenService.GenerateAccessToken(user);

    // Assert
    Assert.IsNotNullOrEmpty(token);
    var principal = tokenService.ValidateToken(token);
    Assert.IsNotNull(principal);
}
```

---

## 🐛 Common Issues & Solutions

### Issue: "Refresh token not found"
**Solution:**
```csharp
// Check if token exists and is valid
var token = await _tokenService.VerifyRefreshTokenAsync(refreshToken, ct);
if (!token)
    return Unauthorized("Invalid or expired token");
```

### Issue: "No SMS provider available"
**Solution:** Configure at least one provider in appsettings.json
```json
"SmsProviders": {
  "Twilio": {
    "AccountSid": "AC...",
    "AuthToken": "token",
    "FromNumber": "+1234567890"
  }
}
```

### Issue: "OTP not received"
**Debug:**
```csharp
// Check provider status
var provider = await _providerFactory.GetAvailableProviderAsync(OtpProvider.Twilio, ct);
if (provider == null)
    Console.WriteLine("No SMS provider available");

// Check OTP in database
var otp = await _otpService.GetLatestOtpAsync(phoneNumber, ct);
Console.WriteLine($"OTP: {otp?.OtpCode}, Provider: {otp?.Provider}");
```

### Issue: "JWT validation failed"
**Solution:**
```csharp
// Verify JWT secret is >= 32 characters
var secret = configuration["JwtSettings:Secret"];
if (secret.Length < 32)
    throw new InvalidOperationException("JWT Secret too short!");

// Verify token hasn't expired
var principal = tokenService.ValidateToken(token);
if (principal == null)
    return Unauthorized("Invalid token");
```

---

## 📊 Database Quick Reference

### user_otps Table
```sql
SELECT * FROM user_otps 
WHERE PhoneNumber = '+919876543210' 
  AND ExpiresAt > GETUTCDATE()
  AND IsVerified = 0;
```

### user_devices Table
```sql
SELECT * FROM user_devices 
WHERE UserId = @userId 
  AND IsActive = 1;
```

### refresh_tokens Table
```sql
SELECT * FROM refresh_tokens 
WHERE UserId = @userId 
  AND RevokedAt IS NULL 
  AND ExpiresAt > GETUTCDATE();
```

### Cleanup Expired OTPs
```sql
DELETE FROM user_otps 
WHERE ExpiresAt < GETUTCDATE() 
  AND IsVerified = 0;
```

---

## 🔄 Authentication Flow Diagram

```
┌─────────┐
│ CLIENT  │
└────┬────┘
     │
     │ 1. POST /register
     ▼
┌────────────────┐        ┌──────────────┐
│ AuthController │───────►│ AuthService  │
└─────────────────────────┴──────────────┘
     │                          │
     │                 2. OtpService.Generate
     │                          │
     │                    ┌─────▼──────┐
     │                    │ SmsProvider│
     │                    │  (Twilio)  │
     │                    └────────────┘
     │                          │
     │◄─────────────────────────┘
     │ 3. OTP sent to phone
     │
     │ 4. POST /verify-login
     ▼
     │ OtpService.Verify
     │     │
     │     ├─ Check OTP matches
     │     ├─ Check not expired
     │     ├─ Check attempt count
     │     └─ Mark as verified
     │
     │ 5. JwtTokenService.GenerateTokenResponse
     │     │
     │     ├─ Generate access token (15 min)
     │     ├─ Generate refresh token (7 day)
     │     └─ Store refresh token in DB
     │
     │◄─────────────────────────
     │ 6. Return tokens
     │
     │ 7. GET /profile
     │    Header: Authorization: Bearer {accessToken}
     │
     ▼ (JWT middleware validates token)
     │
     ├─ Validate signature
     ├─ Check expiration
     ├─ Extract claims
     └─ Allow access
```

---

## 📝 Common Code Patterns

### Adding New Endpoint with Auth
```csharp
[HttpPost("create-listing")]
[Authorize(Policy = "RequireVendor")]
public async Task<IActionResult> CreateListing(
    [FromBody] CreateListingDto dto,
    CancellationToken ct)
{
    var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
    var user = await _authService.GetUserAsync(userId, ct);
    
    // Create listing for vendor
    var listing = new VendorListing
    {
        VendorUserId = userId,
        // ... other properties
    };
    
    _dbContext.VendorListings.Add(listing);
    await _dbContext.SaveChangesAsync(ct);
    
    return CreatedAtAction(nameof(GetListing), new { id = listing.Id }, listing);
}
```

### Checking User Roles
```csharp
var isVendor = User.IsInRole("Vendor");
var isBuyer = User.IsInRole("Buyer");

if (!isVendor)
    return Forbid("You must be a vendor to perform this action");
```

### Accessing User Info
```csharp
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var name = User.FindFirst(ClaimTypes.Name)?.Value;
var email = User.FindFirst(ClaimTypes.Email)?.Value;
var phone = User.FindFirst("Phone")?.Value;
```

---

## 🎓 Learning Resources

### Files to Read First
1. **QUICK_START_GUIDE.md** - Get started quickly
2. **OTP_AUTHENTICATION_GUIDE.md** - Comprehensive reference
3. **IMPLEMENTATION_CHECKLIST.md** - Understand next phases

### Code Files to Review
1. `Controllers/AuthController.cs` - See API usage
2. `Infrastructure/Services/Auth/AuthenticationService.cs` - Understand flow
3. `Infrastructure/Services/Sms/SmsProviderFactory.cs` - Learn pattern
4. `VendorProject.EF/Models/User*.cs` - Database models

### Key Concepts
- Strategy Pattern (SMS providers)
- Factory Pattern (Provider factory)
- JWT Authentication
- Claims-based authorization
- Database indexing

---

## ✨ Tips & Tricks

### Enable Request Logging
```csharp
// In Program.cs
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

### Test with Mock Provider
- Default is MockSmsSender in development
- OTP codes appear in console output
- OTP returned in API response (development only)

### Bypass Token Expiration (Dev Only)
```csharp
// In appsettings.Development.json
"JwtSettings": {
  "AccessTokenExpirationMinutes": 1440  // 24 hours for testing
}
```

### Debug Claims
```csharp
public IActionResult DebugClaims()
{
    var claims = User.Claims.ToList();
    foreach (var claim in claims)
    {
        Console.WriteLine($"{claim.Type}: {claim.Value}");
    }
    return Ok(claims);
}
```

---

## 📞 Quick Help

**Q: How do I add a new SMS provider?**
A: Create class implementing `ISmsSender`, register in Program.cs, add settings to appsettings.json

**Q: How do I change OTP validity?**
A: Update `OtpSettings.OtpValidityMinutes` in appsettings.json

**Q: How do I change token expiration?**
A: Update `JwtSettings.AccessTokenExpirationMinutes` in appsettings.json

**Q: How do I add a new authorization policy?**
A: Add in Program.cs `builder.Services.AddAuthorization(options => { ... })`

**Q: How do I test endpoints locally?**
A: Use Swagger UI at `https://localhost:7001/swagger/ui.html`

---

**Last Updated:** January 26, 2026
**Version:** 1.0
**Status:** Production Ready ✅

