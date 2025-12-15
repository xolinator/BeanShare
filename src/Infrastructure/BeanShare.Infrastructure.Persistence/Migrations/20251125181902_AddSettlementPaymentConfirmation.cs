using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeanShare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSettlementPaymentConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Settlements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Settlements",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "AwaitingConfirmation");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "SettlementLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConfirmedBy",
                table: "SettlementLines",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "SettlementLines");

            migrationBuilder.DropColumn(
                name: "ConfirmedBy",
                table: "SettlementLines");
        }
    }
}
