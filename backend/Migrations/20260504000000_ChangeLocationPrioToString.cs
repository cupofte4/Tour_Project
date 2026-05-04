using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLocationPrioToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Prio",
                table: "Locations",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Silver",
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("""
                UPDATE Locations
                SET Prio = CASE
                    WHEN Prio IN ('Premium', '10') THEN 'Premium'
                    WHEN Prio IN ('Gold', '8', '6') THEN 'Gold'
                    WHEN Prio IN ('Silver', '4', '2', '0') THEN 'Silver'
                    ELSE 'Silver'
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Locations
                SET Prio = CASE
                    WHEN Prio = 'Premium' THEN '10'
                    WHEN Prio = 'Gold' THEN '6'
                    ELSE '2'
                END
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Prio",
                table: "Locations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Silver")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
