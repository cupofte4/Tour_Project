using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Tour_Project.Data;

#nullable disable

namespace backend.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260424124500_AddWebAnalyticsTables")]
    public partial class AddWebAnalyticsTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS `VisitorDevices` (
                    `Id` bigint NOT NULL AUTO_INCREMENT,
                    `DeviceId` varchar(128) NOT NULL,
                    `FirstSeenAtUtc` datetime(6) NOT NULL,
                    `LastSeenAtUtc` datetime(6) NOT NULL,
                    `LastPath` varchar(1024) NOT NULL,
                    `LastUserAgent` varchar(512) NOT NULL,
                    CONSTRAINT `PK_VisitorDevices` PRIMARY KEY (`Id`)
                );

                CREATE TABLE IF NOT EXISTS `AnalyticsEvents` (
                    `Id` bigint NOT NULL AUTO_INCREMENT,
                    `DeviceId` varchar(128) NOT NULL,
                    `EventType` varchar(64) NOT NULL,
                    `LocationId` int NULL,
                    `Path` varchar(1024) NOT NULL,
                    `CreatedAtUtc` datetime(6) NOT NULL,
                    CONSTRAINT `PK_AnalyticsEvents` PRIMARY KEY (`Id`)
                );

                CREATE TABLE IF NOT EXISTS `LocationFavoriteStates` (
                    `Id` bigint NOT NULL AUTO_INCREMENT,
                    `DeviceId` varchar(128) NOT NULL,
                    `LocationId` int NOT NULL,
                    `IsFavorited` tinyint(1) NOT NULL,
                    `FavoritedAtUtc` datetime(6) NULL,
                    `UpdatedAtUtc` datetime(6) NOT NULL,
                    CONSTRAINT `PK_LocationFavoriteStates` PRIMARY KEY (`Id`)
                );

                SET @idx_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'VisitorDevices' AND INDEX_NAME = 'IX_VisitorDevices_DeviceId'
                );
                SET @idx_sql = IF(@idx_exists = 0, 'CREATE UNIQUE INDEX `IX_VisitorDevices_DeviceId` ON `VisitorDevices` (`DeviceId`);', 'SELECT 1;');
                PREPARE idx_stmt FROM @idx_sql; EXECUTE idx_stmt; DEALLOCATE PREPARE idx_stmt;

                SET @idx_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'VisitorDevices' AND INDEX_NAME = 'IX_VisitorDevices_LastSeenAtUtc'
                );
                SET @idx_sql = IF(@idx_exists = 0, 'CREATE INDEX `IX_VisitorDevices_LastSeenAtUtc` ON `VisitorDevices` (`LastSeenAtUtc`);', 'SELECT 1;');
                PREPARE idx_stmt FROM @idx_sql; EXECUTE idx_stmt; DEALLOCATE PREPARE idx_stmt;

                SET @idx_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AnalyticsEvents' AND INDEX_NAME = 'IX_AnalyticsEvents_DeviceId_CreatedAtUtc'
                );
                SET @idx_sql = IF(@idx_exists = 0, 'CREATE INDEX `IX_AnalyticsEvents_DeviceId_CreatedAtUtc` ON `AnalyticsEvents` (`DeviceId`, `CreatedAtUtc`);', 'SELECT 1;');
                PREPARE idx_stmt FROM @idx_sql; EXECUTE idx_stmt; DEALLOCATE PREPARE idx_stmt;

                SET @idx_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AnalyticsEvents' AND INDEX_NAME = 'IX_AnalyticsEvents_EventType_CreatedAtUtc'
                );
                SET @idx_sql = IF(@idx_exists = 0, 'CREATE INDEX `IX_AnalyticsEvents_EventType_CreatedAtUtc` ON `AnalyticsEvents` (`EventType`, `CreatedAtUtc`);', 'SELECT 1;');
                PREPARE idx_stmt FROM @idx_sql; EXECUTE idx_stmt; DEALLOCATE PREPARE idx_stmt;

                SET @idx_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'LocationFavoriteStates' AND INDEX_NAME = 'IX_LocationFavoriteStates_DeviceId_LocationId'
                );
                SET @idx_sql = IF(@idx_exists = 0, 'CREATE UNIQUE INDEX `IX_LocationFavoriteStates_DeviceId_LocationId` ON `LocationFavoriteStates` (`DeviceId`, `LocationId`);', 'SELECT 1;');
                PREPARE idx_stmt FROM @idx_sql; EXECUTE idx_stmt; DEALLOCATE PREPARE idx_stmt;

                SET @idx_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'LocationFavoriteStates' AND INDEX_NAME = 'IX_LocationFavoriteStates_IsFavorited'
                );
                SET @idx_sql = IF(@idx_exists = 0, 'CREATE INDEX `IX_LocationFavoriteStates_IsFavorited` ON `LocationFavoriteStates` (`IsFavorited`);', 'SELECT 1;');
                PREPARE idx_stmt FROM @idx_sql; EXECUTE idx_stmt; DEALLOCATE PREPARE idx_stmt;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TABLE IF EXISTS `LocationFavoriteStates`;
                DROP TABLE IF EXISTS `AnalyticsEvents`;
                DROP TABLE IF EXISTS `VisitorDevices`;
                """);
        }
    }
}
