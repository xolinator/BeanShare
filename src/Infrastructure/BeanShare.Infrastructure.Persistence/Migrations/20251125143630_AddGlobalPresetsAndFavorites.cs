using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeanShare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalPresetsAndFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlobalPresets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DefaultCoffeeType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DefaultBrand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DefaultPreparation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DefaultGrams = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalPresets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpaceGlobalPresetConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    GlobalPresetId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaceGlobalPresetConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPresetFavorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    GlobalPresetId = table.Column<Guid>(type: "uuid", nullable: true),
                    PresetRecipeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPresetFavorites", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GlobalPresets_DisplayOrder",
                table: "GlobalPresets",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalPresets_IsActive",
                table: "GlobalPresets",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceGlobalPresetConfigs_SpaceId",
                table: "SpaceGlobalPresetConfigs",
                column: "SpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceGlobalPresetConfigs_SpaceId_GlobalPresetId",
                table: "SpaceGlobalPresetConfigs",
                columns: new[] { "SpaceId", "GlobalPresetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPresetFavorites_UserId_SpaceId",
                table: "UserPresetFavorites",
                columns: new[] { "UserId", "SpaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPresetFavorites_UserId_SpaceId_GlobalPresetId",
                table: "UserPresetFavorites",
                columns: new[] { "UserId", "SpaceId", "GlobalPresetId" },
                unique: true,
                filter: "\"GlobalPresetId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserPresetFavorites_UserId_SpaceId_PresetRecipeId",
                table: "UserPresetFavorites",
                columns: new[] { "UserId", "SpaceId", "PresetRecipeId" },
                unique: true,
                filter: "\"PresetRecipeId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalPresets");

            migrationBuilder.DropTable(
                name: "SpaceGlobalPresetConfigs");

            migrationBuilder.DropTable(
                name: "UserPresetFavorites");
        }
    }
}
