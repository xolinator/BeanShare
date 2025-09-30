using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeanShare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsumptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Consumptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProductBrand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProductType = table.Column<string>(type: "text", nullable: false),
                    QuantityGrams = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consumptions_ConsumedAt",
                table: "Consumptions",
                column: "ConsumedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Consumptions_SpaceId",
                table: "Consumptions",
                column: "SpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumptions_SpaceId_ConsumedAt",
                table: "Consumptions",
                columns: new[] { "SpaceId", "ConsumedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Consumptions");
        }
    }
}
