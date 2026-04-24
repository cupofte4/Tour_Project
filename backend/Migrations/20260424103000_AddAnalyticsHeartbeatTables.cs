using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Tour_Project.Data;

#nullable disable

namespace backend.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260424103000_AddAnalyticsHeartbeatTables")]
    public partial class AddAnalyticsHeartbeatTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS `AudioPlays` (
                    `Id` bigint NOT NULL AUTO_INCREMENT,
                    `DeviceId` longtext NOT NULL,
                    `LocationId` int NOT NULL,
                    `AudioId` int NULL,
                    `OccurredAtUtc` datetime(6) NOT NULL,
                    CONSTRAINT `PK_AudioPlays` PRIMARY KEY (`Id`)
                );

                CREATE TABLE IF NOT EXISTS `FavoriteClicks` (
                    `Id` bigint NOT NULL AUTO_INCREMENT,
                    `DeviceId` longtext NOT NULL,
                    `LocationId` int NOT NULL,
                    `IsFavorite` tinyint(1) NOT NULL,
                    `OccurredAtUtc` datetime(6) NOT NULL,
                    CONSTRAINT `PK_FavoriteClicks` PRIMARY KEY (`Id`)
                );

                CREATE TABLE IF NOT EXISTS `AppUsageHeartbeats` (
                    `Id` bigint NOT NULL AUTO_INCREMENT,
                    `SessionId` longtext NOT NULL,
                    `DeviceId` longtext NOT NULL,
                    `OccurredAtUtc` datetime(6) NOT NULL,
                    `Platform` longtext NOT NULL,
                    `AppVersion` longtext NOT NULL,
                    CONSTRAINT `PK_AppUsageHeartbeats` PRIMARY KEY (`Id`)
                );

                SET @heartbeat_index_exists = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'AppUsageHeartbeats'
                      AND INDEX_NAME = 'IX_AppUsageHeartbeats_DeviceId_OccurredAtUtc'
                );
                SET @create_heartbeat_index_sql = IF(
                    @heartbeat_index_exists = 0,
                    'CREATE INDEX `IX_AppUsageHeartbeats_DeviceId_OccurredAtUtc` ON `AppUsageHeartbeats` (`DeviceId`(255), `OccurredAtUtc`);',
                    'SELECT 1;'
                );
                PREPARE create_heartbeat_index_stmt FROM @create_heartbeat_index_sql;
                EXECUTE create_heartbeat_index_stmt;
                DEALLOCATE PREPARE create_heartbeat_index_stmt;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TABLE IF EXISTS `AppUsageHeartbeats`;
                DROP TABLE IF EXISTS `FavoriteClicks`;
                DROP TABLE IF EXISTS `AudioPlays`;
                """);
        }
    }
}
