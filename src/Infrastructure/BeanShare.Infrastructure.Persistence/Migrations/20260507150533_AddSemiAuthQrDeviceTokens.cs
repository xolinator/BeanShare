using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeanShare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSemiAuthQrDeviceTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SemiAuthQrDeviceTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceIdHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    LastUsedSpaceId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastUsedQrCodeId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemiAuthQrDeviceTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SemiAuthQrDeviceTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SemiAuthQrDeviceTokens_TokenHash",
                table: "SemiAuthQrDeviceTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SemiAuthQrDeviceTokens_User_Device",
                table: "SemiAuthQrDeviceTokens",
                columns: new[] { "UserId", "DeviceIdHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SemiAuthQrDeviceTokens_User_Revoked_ExpiresAt",
                table: "SemiAuthQrDeviceTokens",
                columns: new[] { "UserId", "IsRevoked", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SemiAuthQrDeviceTokens");
        }
    }
}
