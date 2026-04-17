using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhKhanhGuide.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppUsageSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsageSessions",
                columns: table => new
                {
                    SessionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AnonymousClientId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastEventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FirstSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AppVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Platform = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsageSessions", x => x.SessionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUsageSessions_AnonymousClientId",
                table: "AppUsageSessions",
                column: "AnonymousClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsageSessions_LastSeenAtUtc",
                table: "AppUsageSessions",
                column: "LastSeenAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUsageSessions");
        }
    }
}
