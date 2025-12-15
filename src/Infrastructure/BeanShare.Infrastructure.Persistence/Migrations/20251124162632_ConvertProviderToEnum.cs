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
            // PostgreSQL requires explicit USING clause for type conversion
            // Handle both string values and the case where it might already be numeric
            migrationBuilder.Sql(@"
                ALTER TABLE ""Users""
                ALTER COLUMN ""Provider"" TYPE integer
                USING CASE
                    WHEN ""Provider"" = 'Email' THEN 0
                    WHEN ""Provider"" = 'Google' THEN 1
                    WHEN ""Provider"" = 'Facebook' THEN 2
                    WHEN ""Provider"" ~ '^\d+$' THEN ""Provider""::integer
                    ELSE 0
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // PostgreSQL requires explicit USING clause for type conversion
            migrationBuilder.Sql(@"
                ALTER TABLE ""Users""
                ALTER COLUMN ""Provider"" TYPE character varying(50)
                USING CASE ""Provider""
                    WHEN 0 THEN 'Email'
                    WHEN 1 THEN 'Google'
                    WHEN 2 THEN 'Facebook'
                    ELSE 'Email'
                END;
            ");
        }
    }
}
