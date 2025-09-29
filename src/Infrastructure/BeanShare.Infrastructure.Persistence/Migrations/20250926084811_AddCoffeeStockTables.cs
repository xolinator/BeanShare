using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeanShare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoffeeStockTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoffeeStocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeStocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProductBrand = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductType = table.Column<string>(type: "text", nullable: false),
                    QuantityGrams = table.Column<decimal>(type: "numeric(10,1)", nullable: false),
                    CostAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CostCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Vendor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PurchasedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CoffeeStockId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchases_CoffeeStocks_CoffeeStockId",
                        column: x => x.CoffeeStockId,
                        principalTable: "CoffeeStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProductBrand = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductType = table.Column<string>(type: "text", nullable: false),
                    TotalPurchasedGrams = table.Column<decimal>(type: "numeric(10,1)", nullable: false),
                    TotalConsumedGrams = table.Column<decimal>(type: "numeric(10,1)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CoffeeStockId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockLevels_CoffeeStocks_CoffeeStockId",
                        column: x => x.CoffeeStockId,
                        principalTable: "CoffeeStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoffeeStocks_SpaceId",
                table: "CoffeeStocks",
                column: "SpaceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_CoffeeStockId",
                table: "Purchases",
                column: "CoffeeStockId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_CreatedAt",
                table: "Purchases",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_PurchasedAt",
                table: "Purchases",
                column: "PurchasedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StockLevels_CoffeeStockId",
                table: "StockLevels",
                column: "CoffeeStockId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLevels_UpdatedAt",
                table: "StockLevels",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "StockLevels");

            migrationBuilder.DropTable(
                name: "CoffeeStocks");
        }
    }
}
