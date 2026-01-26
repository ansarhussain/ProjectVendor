# Quick Start Guide - OTP Authentication System

## Installation & Setup

### 1. Build the Solution
```bash
cd c:\AppDevelopment
dotnet build
```

### 2. Apply Database Migration
```bash
dotnet ef database update -p VendorProject.EF -s VendorProject
```

### 3. Update appsettings.json (Optional - for real SMS providers)
```json
"SmsProviders": {
  "Twilio": {
    "AccountSid": "your-account-sid",
    "AuthToken": "your-auth-token",
    "FromNumber": "+1234567890"
  }
}
```

### 4. Run the Application
```bash
cd VendorProject
dotnet run
```

Visit Swagger UI: `https://localhost:7001/swagger/ui.html`

---

## Complete Registration Flow

### Step 1: Register User
```bash
curl -X POST "https://localhost:7001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+919876543210",
    "fullName": "John Doe",
    "email": "john@example.com",
    "desiredRole": "Buyer"
  }'
```

**Response:**
- OTP is sent to phone
- In development (Mock provider), OTP appears in logs

### Step 2: Verify Registration OTP
```bash
curl -X POST "https://localhost:7001/api/auth/verify-registration" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+919876543210",
    "otpCode": "123456"
  }'
```

---

## Complete Login Flow

### Step 1: Send Login OTP
```bash
curl -X POST "https://localhost:7001/api/auth/send-login-otp" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+919876543210"
  }'
```

### Step 2: Verify Login OTP & Get Tokens
```bash
curl -X POST "https://localhost:7001/api/auth/verify-login" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+919876543210",
    "otpCode": "123456"
  }'
```

**Response includes:**
- `accessToken`: Valid for 15 minutes
- `refreshToken`: Valid for 7 days
- `user`: Complete user profile

---

## Using Access Token

### Add to Request Headers
```bash
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Example - Get User Profile
```bash
curl -X GET "https://localhost:7001/api/auth/profile" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

---

## Token Refresh

When access token expires (15 minutes):

```bash
curl -X POST "https://localhost:7001/api/auth/refresh-token" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

Returns new access token and refresh token.

---

## Logout

```bash
curl -X POST "https://localhost:7001/api/auth/logout" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

---

## Role-Based Access Control

### Example: Only Vendors Can Access
```csharp
[HttpPost("listings")]
[Authorize(Policy = "RequireVendor")]
public async Task<IActionResult> CreateListing([FromBody] CreateListingDto dto)
{
    // Only users with Vendor role
}
```

### Available Policies
- `RequireVendor` - Vendor role
- `RequireBuyer` - Buyer role
- `RequireTransporter` - Transporter role
- `RequireAdmin` - Admin role
- `PhoneVerified` - Phone must be verified
- `KycVerified` - KYC must be verified

---

## Code Examples

### Using Dependency Injection in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    
    public OrderController(IAuthenticationService authService)
    {
        _authService = authService;
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyOrders(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _authService.GetUserAsync(Guid.Parse(userId), ct);
        // ... return user's orders
    }
}
```

### Checking User Roles
```csharp
var isVendor = User.IsInRole("Vendor");
var isBuyer = User.IsInRole("Buyer");
```

### Extracting Claims
```csharp
var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
var phoneNumber = User.FindFirst("Phone")?.Value;
var isPhoneVerified = bool.Parse(User.FindFirst("PhoneVerified")?.Value ?? "false");
```

---

## Configuration Reference

### OTP Settings
| Setting | Default | Description |
|---------|---------|-------------|
| OtpLength | 6 | Number of digits in OTP |
| OtpValidityMinutes | 10 | Minutes until OTP expires |
| MaxAttempts | 3 | Max verification attempts before reset required |
| RateLimitRequestsPerMinute | 3 | Max OTP requests per minute per phone |

### JWT Settings
| Setting | Default | Description |
|---------|---------|-------------|
| Secret | (required) | Secret key for signing tokens - **CHANGE IN PRODUCTION** |
| Issuer | VendorProject | JWT issuer claim |
| Audience | VendorProjectUsers | JWT audience claim |
| AccessTokenExpirationMinutes | 15 | Access token lifetime |
| RefreshTokenExpirationDays | 7 | Refresh token lifetime |

---

## Common Issues

### Issue: "The type or namespace name 'OtpPurpose' does not exist"
**Solution:** Add `using VendorProject.EF.Models;` to file

### Issue: JWT validation failure
**Solution:** 
1. Ensure Secret in appsettings.json is >= 32 characters
2. Check token hasn't expired (15 min default)
3. Verify token hasn't been tampered with

### Issue: SMS not being sent in development
**Solution:** Use MockSmsSender (default in development)
- OTP will be logged to console
- OTP will be returned in API response for testing

### Issue: "Refresh token not found or revoked"
**Solution:**
1. Refresh token is 7 days old - need to login again
2. User logged out - refresh tokens were revoked
3. Database not updated - ensure migration ran

---

## Files Modified/Created

### New Files Created
- `Infrastructure/Services/Sms/ISmsSender.cs`
- `Infrastructure/Services/Sms/SmsProviderFactory.cs`
- `Infrastructure/Services/Sms/TwilioSmsSender.cs`
- `Infrastructure/Services/Sms/VonageSmsSender.cs`
- `Infrastructure/Services/Sms/AwsSnsSmsSender.cs`
- `Infrastructure/Services/Sms/MockSmsSender.cs`
- `Infrastructure/Services/Auth/OtpService.cs`
- `Infrastructure/Services/Auth/JwtTokenService.cs`
- `Infrastructure/Services/Auth/AuthenticationService.cs`
- `Controllers/AuthController.cs`
- `VendorProject.EF/Models/UserOtp.cs`
- `VendorProject.EF/Models/UserDevice.cs`
- `VendorProject.EF/Models/RefreshToken.cs`
- `VendorProject.EF/Migrations/20260126000000_AddOtpAuthenticationModels.cs`

### Modified Files
- `VendorProject/Program.cs` - Added JWT, OTP, SMS DI & middleware
- `VendorProject/appsettings.json` - Added JWT & SMS settings
- `VendorProject/VendorProject.csproj` - Added JWT NuGet packages
- `VendorProject.EF/Models/User.cs` - Added auth-related fields
- `VendorProject.EF/Data/MarketplaceDbContext.cs` - Added new DbSets
- `VendorProject.EF/Data/EntityConfigurations.cs` - Added new configurations

---

## Support

For questions or issues:
1. Check the **OTP_AUTHENTICATION_GUIDE.md** for detailed documentation
2. Review API examples in this guide
3. Check logs in console output for diagnostic information

---

## Next Steps

1. ✅ Database migration applied
2. ✅ Build solution successfully
3. 📋 TODO: Create user entity after OTP verification
4. 📋 TODO: Implement KYC verification flow
5. 📋 TODO: Add password reset functionality
6. 📋 TODO: Integrate with real SMS providers
7. 📋 TODO: Setup production JWT secret management

