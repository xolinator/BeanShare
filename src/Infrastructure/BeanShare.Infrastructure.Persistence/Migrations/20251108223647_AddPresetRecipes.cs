using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeanShare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPresetRecipes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Spaces",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PresetRecipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CoffeeType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Preparation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DefaultGrams = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsShared = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    UsageCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresetRecipes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PresetRecipes_SpaceId",
                table: "PresetRecipes",
                column: "SpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_PresetRecipes_SpaceId_UsageCount",
                table: "PresetRecipes",
                columns: new[] { "SpaceId", "UsageCount" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PresetRecipes_UserId",
                table: "PresetRecipes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PresetRecipes");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Spaces");
        }
    }
}
