using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeanShare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignCurrentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SpaceMemberships_UserId",
                table: "SpaceMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumptions_BillingPeriodId",
                table: "Consumptions",
                column: "BillingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumptions_UserId",
                table: "Consumptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SpaceMemberships_UserId",
                table: "SpaceMemberships");

            migrationBuilder.DropIndex(
                name: "IX_Consumptions_BillingPeriodId",
                table: "Consumptions");

            migrationBuilder.DropIndex(
                name: "IX_Consumptions_UserId",
                table: "Consumptions");
        }
    }
}
