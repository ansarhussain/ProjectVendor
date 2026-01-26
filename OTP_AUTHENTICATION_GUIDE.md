# OTP Authentication Implementation Guide

## Overview
Complete OTP-based authentication system with multi-SMS provider support, JWT tokens, role-based access control, and device fingerprinting for the VendorProject B2B marketplace.

---

## Architecture

### Design Patterns Used

1. **Strategy Pattern**: SMS provider abstraction (`ISmsSender`) allowing runtime provider selection
2. **Factory Pattern**: `SmsProviderFactory` for managing multiple SMS providers and fallback logic
3. **Repository Pattern**: EF Core DbContext for data access
4. **Dependency Injection**: Full DI container configuration in Program.cs
5. **Middleware Pattern**: JWT bearer token authentication and authorization middleware

---

## Project Structure

```
VendorProject/
├── Infrastructure/
│   └── Services/
│       ├── Auth/
│       │   ├── AuthenticationService.cs       # Login/registration orchestration
│       │   ├── IJwtTokenService.cs            # JWT token generation interface
│       │   ├── JwtTokenService.cs             # JWT implementation
│       │   ├── OtpService.cs                  # OTP generation & verification
│       │   └── IOtpService.cs                 # OTP service interface
│       └── Sms/
│           ├── ISmsSender.cs                  # SMS provider interface
│           ├── TwilioSmsSender.cs             # Twilio implementation
│           ├── VonageSmsSender.cs             # Vonage implementation
│           ├── AwsSnsSmsSender.cs             # AWS SNS implementation
│           ├── MockSmsSender.cs               # Mock for testing/development
│           └── SmsProviderFactory.cs          # Provider factory & fallback logic
├── Controllers/
│   └── AuthController.cs                      # Authentication endpoints
├── Program.cs                                 # DI registration & middleware config
└── appsettings.json                          # JWT & SMS provider settings
│
VendorProject.EF/
├── Models/
│   ├── User.cs (extended)                     # Added phone verification & KYC flags
│   ├── UserOtp.cs                             # OTP records with attempt tracking
│   ├── UserDevice.cs                          # Device fingerprinting & tracking
│   └── RefreshToken.cs                        # Refresh token storage & revocation
├── Data/
│   ├── MarketplaceDbContext.cs (updated)      # Added new DbSets
│   └── EntityConfigurations.cs (updated)      # Configurations for new models
└── Migrations/
    └── 20260126000000_AddOtpAuthenticationModels.cs
```

---

## Database Schema

### New Tables

#### `user_otps`
| Column | Type | Notes |
|--------|------|-------|
| Id | uniqueidentifier | PK |
| UserId | uniqueidentifier | FK to users |
| PhoneNumber | nvarchar(20) | Denormalized for lookup |
| OtpCode | nvarchar(10) | 6-digit code |
| Provider | nvarchar(30) | SMS provider used (enum) |
| Purpose | nvarchar(30) | Registration, Login, PasswordReset |
| IsVerified | bit | Verification status |
| AttemptCount | int | Current attempt number |
| MaxAttempts | int | Default 3 |
| CreatedAt | datetime2 | Auto-set to SYSUTCDATETIME() |
| ExpiresAt | datetime2 | TTL for OTP validity |
| VerifiedAt | datetime2 | Timestamp of verification |

**Indexes:**
- `(PhoneNumber, ExpiresAt)` - OTP lookup
- `(UserId, Purpose, IsVerified)` - User OTP filtering
- `(ExpiresAt)` - Cleanup queries

#### `user_devices`
| Column | Type | Notes |
|--------|------|-------|
| Id | uniqueidentifier | PK |
| UserId | uniqueidentifier | FK to users |
| DeviceName | nvarchar(100) | User-friendly name |
| DeviceId | nvarchar(255) | Unique fingerprint/IMEI |
| DeviceType | nvarchar(50) | Mobile, Tablet, Web |
| IpAddress | nvarchar(45) | IPv4 or IPv6 |
| UserAgent | nvarchar(500) | Browser/app info |
| IsVerified | bit | Device trust status |
| IsActive | bit | Current device status |
| CreatedAt | datetime2 | Auto-set to SYSUTCDATETIME() |
| LastAccessedAt | datetime2 | Last login timestamp |

**Indexes:**
- `(UserId, IsActive)` - Active device queries
- `(DeviceId, UserId)` UNIQUE - Device lookup

#### `refresh_tokens`
| Column | Type | Notes |
|--------|------|-------|
| Id | uniqueidentifier | PK |
| UserId | uniqueidentifier | FK to users |
| Token | nvarchar(500) | Base64-encoded token |
| JwtTokenId | nvarchar(100) | jti claim reference |
| CreatedAt | datetime2 | Auto-set to SYSUTCDATETIME() |
| ExpiresAt | datetime2 | 7-day default |
| RevokedAt | datetime2 | Null if active |

**Indexes:**
- `(Token)` UNIQUE - Token lookup
- `(JwtTokenId)` UNIQUE - Token ID reference
- `(UserId, ExpiresAt)` - User token queries
- `(ExpiresAt)` - Cleanup queries

### User Table Updates
Added columns:
- `IsPhoneVerified` (bit, default false)
- `PhoneVerifiedAt` (datetime2, nullable)
- `IsKycVerified` (bit, default false)
- `KycVerifiedAt` (datetime2, nullable)
- `LastLoginAt` (datetime2, nullable)

---

## API Endpoints

### Authentication Flow

#### 1. Registration with OTP
**POST /api/auth/register**
```json
{
  "phoneNumber": "+919876543210",
  "fullName": "John Doe",
  "email": "john@example.com",
  "desiredRole": "Buyer"
}
```
**Response (200)**
```json
{
  "success": true,
  "message": "OTP sent successfully. Please verify to complete registration.",
  "data": {
    "success": true,
    "message": "OTP sent successfully. Please verify to complete registration.",
    "userId": null,
    "user": {
      "phone": "+919876543210",
      "fullName": "John Doe"
    }
  },
  "timestamp": "2026-01-26T12:00:00Z"
}
```

#### 2. Verify Registration OTP
**POST /api/auth/verify-registration**
```json
{
  "phoneNumber": "+919876543210",
  "otpCode": "123456"
}
```
**Response (200)**
```json
{
  "success": true,
  "message": "Phone number verified successfully. Please complete your profile.",
  "data": {},
  "timestamp": "2026-01-26T12:00:00Z"
}
```

#### 3. Login - Send OTP
**POST /api/auth/send-login-otp**
```json
{
  "phoneNumber": "+919876543210"
}
```
**Response (200)**
```json
{
  "success": true,
  "message": "OTP sent successfully. Please verify to login.",
  "data": {
    "success": true,
    "message": "OTP sent successfully. Please verify to login.",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "fullName": "John Doe",
      "phone": "+919876543210",
      "email": "john@example.com",
      "isPhoneVerified": true,
      "isKycVerified": false,
      "roles": ["Buyer"],
      "createdAt": "2026-01-20T10:00:00Z",
      "lastLoginAt": null
    }
  },
  "timestamp": "2026-01-26T12:00:00Z"
}
```

#### 4. Verify Login OTP & Get Tokens
**POST /api/auth/verify-login**
```json
{
  "phoneNumber": "+919876543210",
  "otpCode": "123456"
}
```
**Response (200)**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "success": true,
    "message": "Login successful",
    "token": {
      "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "refreshToken": "AbCdEfGhIjKlMnOpQrStUvWxYz...",
      "expiresIn": 900,
      "tokenType": "Bearer"
    },
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "fullName": "John Doe",
      "phone": "+919876543210",
      "email": "john@example.com",
      "isPhoneVerified": true,
      "isKycVerified": false,
      "roles": ["Buyer"],
      "createdAt": "2026-01-20T10:00:00Z",
      "lastLoginAt": "2026-01-26T12:05:00Z"
    }
  },
  "timestamp": "2026-01-26T12:05:00Z"
}
```

#### 5. Refresh Access Token
**POST /api/auth/refresh-token**
```json
{
  "refreshToken": "AbCdEfGhIjKlMnOpQrStUvWxYz..."
}
```
**Response (200)**
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "NewRefreshToken...",
    "expiresIn": 900,
    "tokenType": "Bearer"
  },
  "timestamp": "2026-01-26T12:10:00Z"
}
```

#### 6. Get User Profile
**GET /api/auth/profile**
**Headers:** `Authorization: Bearer {accessToken}`

**Response (200)**
```json
{
  "success": true,
  "message": "User profile retrieved successfully",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "fullName": "John Doe",
    "phone": "+919876543210",
    "email": "john@example.com",
    "isPhoneVerified": true,
    "isKycVerified": false,
    "roles": ["Buyer"],
    "createdAt": "2026-01-20T10:00:00Z",
    "lastLoginAt": "2026-01-26T12:05:00Z"
  },
  "timestamp": "2026-01-26T12:10:00Z"
}
```

#### 7. Logout
**POST /api/auth/logout**
**Headers:** `Authorization: Bearer {accessToken}`

**Response (200)**
```json
{
  "success": true,
  "message": "Logged out successfully",
  "timestamp": "2026-01-26T12:15:00Z"
}
```

---

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DYNAMIC25;Database=MarketplaceDb;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-change-this-in-production-minimum-32-characters!",
    "Issuer": "VendorProject",
    "Audience": "VendorProjectUsers",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "OtpSettings": {
    "OtpLength": 6,
    "OtpValidityMinutes": 10,
    "MaxAttempts": 3,
    "RateLimitRequestsPerMinute": 3
  },
  "SmsProviders": {
    "Twilio": {
      "AccountSid": "your-account-sid",
      "AuthToken": "your-auth-token",
      "FromNumber": "+1234567890"
    },
    "Vonage": {
      "ApiKey": "your-api-key",
      "ApiSecret": "your-api-secret",
      "FromName": "VendorProject"
    },
    "AwsSns": {
      "AccessKey": "your-access-key",
      "SecretKey": "your-secret-key",
      "Region": "us-east-1"
    }
  }
}
```

---

## JWT Token Claims

### Access Token Payload
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
  "jti": "unique-token-id",
  "exp": 1706251800,
  "iat": 1706250900,
  "nbf": 1706250900
}
```

---

## Authorization Policies

Configured policies in Program.cs:

```csharp
options.AddPolicy("RequireVendor", policy =>
    policy.RequireClaim(ClaimTypes.Role, "Vendor"));

options.AddPolicy("RequireBuyer", policy =>
    policy.RequireClaim(ClaimTypes.Role, "Buyer"));

options.AddPolicy("RequireTransporter", policy =>
    policy.RequireClaim(ClaimTypes.Role, "Transporter"));

options.AddPolicy("RequireAdmin", policy =>
    policy.RequireClaim(ClaimTypes.Role, "Admin"));

options.AddPolicy("PhoneVerified", policy =>
    policy.RequireClaim("PhoneVerified", "True"));

options.AddPolicy("KycVerified", policy =>
    policy.RequireClaim("KycVerified", "True"));
```

### Usage Example

```csharp
[HttpPost("vendor-listing")]
[Authorize(Policy = "RequireVendor")]
public async Task<IActionResult> CreateVendorListing(...)
{
    // Only vendors with verified phone can create listings
}
```

---

## SMS Provider Integration

### Strategy Pattern Flow

```
User Request
    ↓
AuthController
    ↓
AuthenticationService.LoginAsync()
    ↓
OtpService.GenerateAndSendOtpAsync()
    ↓
SmsProviderFactory.GetAvailableProviderAsync()
    ├─ Try Preferred Provider (Twilio)
    │   └─ If available → Use it
    │   └─ If unavailable → Try fallback
    ├─ Try First Available (Vonage)
    │   └─ If available → Use it
    │   └─ If unavailable → Try fallback
    ├─ Try Second Available (AWS SNS)
    │   └─ If available → Use it
    └─ If none available → Return error
```

### Adding New SMS Provider

1. Create new class implementing `ISmsSender`:
```csharp
public class NewProviderSmsSender : ISmsSender
{
    public string ProviderName => "NewProvider";
    
    public async Task<bool> SendOtpAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        // Implementation
    }
    
    public Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        // Availability check
    }
}
```

2. Register in Program.cs:
```csharp
var newProviderSettings = builder.Configuration.GetSection("SmsProviders:NewProvider").Get<NewProviderSettings>();
if (newProviderSettings?.IsConfigured == true)
{
    builder.Services.AddSingleton<ISmsSender>(sp => 
        new NewProviderSmsSender(sp.GetRequiredService<ILogger<NewProviderSmsSender>>(), newProviderSettings));
}
```

3. Add settings to appsettings.json:
```json
"SmsProviders": {
  "NewProvider": {
    "Setting1": "value1",
    "Setting2": "value2"
  }
}
```

---

## Service Layer

### OtpService

- **GenerateAndSendOtpAsync**: Generates 6-digit OTP, invalidates previous ones, sends via available SMS provider
- **VerifyOtpAsync**: Validates OTP code, increments attempts, marks as verified
- **GetLatestOtpAsync**: Retrieves latest OTP for a phone (testing/development)
- **CleanupExpiredOtpsAsync**: Scheduled task to remove expired OTPs

### JwtTokenService

- **GenerateAccessToken**: Creates 15-minute JWT with user roles & verification status
- **GenerateRefreshTokenAsync**: Creates 7-day refresh token stored in database
- **ValidateToken**: Validates JWT signature, issuer, audience, lifetime
- **VerifyRefreshTokenAsync**: Checks if refresh token is valid & not revoked
- **RevokeRefreshTokenAsync**: Marks refresh token as revoked (logout)
- **GenerateTokenResponseAsync**: Returns both access and refresh tokens
- **CleanupExpiredTokensAsync**: Scheduled task to remove expired tokens

### AuthenticationService

- **RegisterAsync**: OTP registration flow
- **LoginAsync**: Initiates login with OTP send
- **CompleteLoginAsync**: Verifies OTP and issues tokens
- **RefreshTokenAsync**: Issues new tokens using refresh token
- **LogoutAsync**: Revokes all user's refresh tokens
- **GetUserAsync**: Retrieves user profile

---

## Security Considerations

### Implemented

✅ OTP rate limiting (3 requests per minute per phone)
✅ Max OTP attempt limiting (3 attempts, then reset required)
✅ OTP expiration (10 minutes default)
✅ Refresh token revocation on logout
✅ Database-backed refresh tokens (no stateless JWT refresh)
✅ Device fingerprinting table for future multi-device support
✅ JWT validation with signature verification
✅ Claim-based authorization policies
✅ Nullable PasswordHash (supports phone-only auth)

### Recommended Additional Measures

1. **Rate Limiting per IP**: Implement global API rate limiting
2. **Two-Factor Authentication**: After OTP, optionally require authenticator app
3. **Device Trust**: Implement device verification flow before allowing sensitive operations
4. **Audit Logging**: Log all authentication events for compliance
5. **Session Management**: Track active sessions per user/device
6. **Password Reset Flow**: Implement forgot password with OTP
7. **Email Verification**: Send verification email on KYC verification
8. **Phone Number Masking**: Mask phone numbers in logs/responses for PII protection
9. **Environment-Specific Secrets**: Use Azure Key Vault or AWS Secrets Manager in production
10. **HTTPS Only**: Ensure all endpoints require HTTPS

---

## Testing

### Mock Provider Usage

For development/testing, the `MockSmsSender` is automatically registered and:
- Logs OTP codes to application logs (console)
- Returns OTP in API response (testing convenience)
- Simulates 100ms network delay

### Test Flow Example

```csharp
// 1. Register
POST /api/auth/register
{ "phoneNumber": "+919876543210", "fullName": "Test User" }

// Response contains mock OTP in logs: "MOCK SMS to +919876543210: Your OTP is: 123456..."

// 2. Send login OTP
POST /api/auth/send-login-otp
{ "phoneNumber": "+919876543210" }

// 3. Verify login with OTP
POST /api/auth/verify-login
{ "phoneNumber": "+919876543210", "otpCode": "123456" }

// Response includes JWT tokens for authenticated requests
```

---

## Database Migration

Run the migration to create new tables and columns:

```bash
dotnet ef database update
```

This will:
1. Add `user_otps` table with indexes
2. Add `user_devices` table with indexes
3. Add `refresh_tokens` table with indexes
4. Add columns to `users` table: `IsPhoneVerified`, `PhoneVerifiedAt`, `IsKycVerified`, `KycVerifiedAt`, `LastLoginAt`

---

## Future Enhancements

1. **Multi-Device Management**: Allow users to manage trusted devices
2. **Passwordless Login**: Complete phone-only authentication without password
3. **Social Login**: Integrate Google/Facebook login alongside OTP
4. **Biometric Authentication**: Face ID / Fingerprint on mobile apps
5. **SMS Backup**: Email OTP if SMS fails
6. **OTP Delivery Methods**: Push notifications as OTP alternative
7. **Role-Specific Onboarding**: Custom KYC flows per role
8. **Referral System**: Assign referral codes during registration
9. **Device Recognition**: Auto-verify devices based on trust score
10. **Analytics**: Track authentication metrics (success rate, OTP delivery time, etc.)

---

## Dependencies Added

- `Microsoft.AspNetCore.Authentication.JwtBearer` v8.0.0
- `System.IdentityModel.Tokens.Jwt` v7.0.3

---

## Clean Code Principles Applied

✅ **Single Responsibility**: Each service has one clear purpose
✅ **Open/Closed Principle**: SMS providers extensible via interface
✅ **Dependency Inversion**: Depends on abstractions, not concrete implementations
✅ **Repository Pattern**: Data access abstracted through DbContext
✅ **SOLID Design**: Follows SOLID principles throughout
✅ **Async/Await**: All I/O operations are async
✅ **Error Handling**: Comprehensive exception handling with logging
✅ **Validation**: Input validation at service and controller layers
✅ **Documentation**: XML comments on public methods
✅ **Configuration Management**: Externalized settings via appsettings.json

---

## Next Steps

1. **Update User Creation**: Modify user registration to create User entity after OTP verification
2. **Implement KYC Flow**: Add endpoint for KYC document submission after registration
3. **Add Refresh Token Cleanup Job**: Schedule background task to clean expired tokens
4. **Configure Real SMS Providers**: Update appsettings.json with Twilio/Vonage credentials
5. **Implement Password Reset**: Add forgot password flow using OTP
6. **Add Email Notifications**: Send welcome email on successful registration
7. **Setup Audit Logging**: Track all authentication events
8. **Add Rate Limiting Middleware**: Implement global API rate limiting
9. **Create Integration Tests**: Write tests for authentication flows
10. **Production Deployment**: Set up secrets management and SSL certificates

