using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    public partial class AddLocationPriority : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET @prio_exists = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Locations'
                      AND COLUMN_NAME = 'Prio'
                );
                SET @add_prio_sql = IF(
                    @prio_exists = 0,
                    'ALTER TABLE `Locations` ADD COLUMN `Prio` int NOT NULL DEFAULT 0;',
                    'SELECT 1;'
                );
                PREPARE add_prio_stmt FROM @add_prio_sql;
                EXECUTE add_prio_stmt;
                DEALLOCATE PREPARE add_prio_stmt;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET @prio_exists = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Locations'
                      AND COLUMN_NAME = 'Prio'
                );
                SET @drop_prio_sql = IF(
                    @prio_exists = 1,
                    'ALTER TABLE `Locations` DROP COLUMN `Prio`;',
                    'SELECT 1;'
                );
                PREPARE drop_prio_stmt FROM @drop_prio_sql;
                EXECUTE drop_prio_stmt;
                DEALLOCATE PREPARE drop_prio_stmt;
                """);
        }
    }
}
