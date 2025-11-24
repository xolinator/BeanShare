using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeanShare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConvertProviderToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""Provider"" = CASE ""Provider""
                    WHEN 'Email' THEN '0'
                    WHEN 'Google' THEN '1'
                    WHEN 'Facebook' THEN '2'
                    ELSE '0'
                END;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "Provider",
                table: "Users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""Provider"" = CASE ""Provider""
                    WHEN '0' THEN 'Email'
                    WHEN '1' THEN 'Google'
                    WHEN '2' THEN 'Facebook'
                    ELSE 'Email'
                END;
            ");
        }
    }
}
