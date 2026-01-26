using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorProject.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpAuthenticationModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add columns to users table
            migrationBuilder.AddColumn<bool>(
                name: "IsPhoneVerified",
                table: "users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PhoneVerifiedAt",
                table: "users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsKycVerified",
                table: "users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "KycVerifiedAt",
                table: "users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "users",
                type: "datetime2",
                nullable: true);

            // Create user_otps table
            migrationBuilder.CreateTable(
                name: "user_otps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OtpCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_otps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_otps_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create user_devices table
            migrationBuilder.CreateTable(
                name: "user_devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DeviceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_devices_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create refresh_tokens table
            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    JwtTokenId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_user_otps_PhoneNumber_ExpiresAt",
                table: "user_otps",
                columns: new[] { "PhoneNumber", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_user_otps_UserId_Purpose_IsVerified",
                table: "user_otps",
                columns: new[] { "UserId", "Purpose", "IsVerified" });

            migrationBuilder.CreateIndex(
                name: "IX_user_otps_ExpiresAt",
                table: "user_otps",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_user_devices_UserId_IsActive",
                table: "user_devices",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_user_devices_DeviceId_UserId",
                table: "user_devices",
                columns: new[] { "DeviceId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Token",
                table: "refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_JwtTokenId",
                table: "refresh_tokens",
                column: "JwtTokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId_ExpiresAt",
                table: "refresh_tokens",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_ExpiresAt",
                table: "refresh_tokens",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop refresh_tokens table
            migrationBuilder.DropTable(
                name: "refresh_tokens");

            // Drop user_devices table
            migrationBuilder.DropTable(
                name: "user_devices");

            // Drop user_otps table
            migrationBuilder.DropTable(
                name: "user_otps");

            // Remove columns from users table
            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "KycVerifiedAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IsKycVerified",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PhoneVerifiedAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IsPhoneVerified",
                table: "users");
        }
    }
}
