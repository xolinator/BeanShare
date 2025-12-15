using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeanShare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPresetInfoToConsumption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PresetId",
                table: "Consumptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PresetName",
                table: "Consumptions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PresetId",
                table: "Consumptions");

            migrationBuilder.DropColumn(
                name: "PresetName",
                table: "Consumptions");
        }
    }
}
