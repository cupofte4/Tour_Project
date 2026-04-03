using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VinhKhanhGuide.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateStalls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stalls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DescriptionVi = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    TriggerRadiusMeters = table.Column<double>(type: "double precision", precision: 8, scale: 2, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OpenHours = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stalls", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Stalls",
                columns: new[] { "Id", "AverageRating", "Category", "DescriptionVi", "ImageUrl", "Latitude", "Longitude", "Name", "OpenHours", "TriggerRadiusMeters" },
                values: new object[,]
                {
                    { 1, 4.50m, "Hai san", "Quan oc mau sample noi bat voi cac mon oc xao bo toi va oc huong nuong phuc vu khach du lich.", "https://example.com/sample-stalls/oc-co-lan.jpg", 10.76042, 106.69320999999999, "Oc Co Lan", "16:00-23:00", 40.0 },
                    { 2, 4.20m, "Do nuong", "Diem nuong sample tren duong Vinh Khanh voi thit nuong, hai san nuong va khong gian ngoai troi don gian.", "https://example.com/sample-stalls/nuong-gio-dem.jpg", 10.76088, 106.69401999999999, "Nuong Gio Dem", "17:00-23:30", 45.0 },
                    { 3, 4.70m, "Hai san", "Quan sample chuyen cua rang me, tom xoc toi va cac mon hai san dam vi phu hop de demo danh sach gian hang.", "https://example.com/sample-stalls/cua-rang-me.jpg", 10.759969999999999, 106.69374999999999, "So 1 Cua Rang Me", "15:30-22:30", 35.0 },
                    { 4, 4.10m, "Lau", "Quan lau sample gia dinh voi cac mon lau hai san, lau thai va lau bo duoc tao de seed du lieu khoi dau.", "https://example.com/sample-stalls/lau-dem-vinh-khanh.jpg", 10.760109999999999, 106.69450999999999, "Lau Dem Vinh Khanh", "16:30-23:00", 50.0 },
                    { 5, 4.00m, "An vat", "Xe banh trang nuong sample nho gon, phuc vu mon an vat buoi toi voi trung, pho mai va kho bo.", "https://example.com/sample-stalls/banh-trang-nuong-198.jpg", 10.76102, 106.69347999999999, "Banh Trang Nuong 198", "18:00-22:00", 30.0 },
                    { 6, 4.30m, "Mon nuoc", "Quan sample phuc vu chao muc nong, muc chien va mon an nhe phu hop khach tham quan cuoi buoi.", "https://example.com/sample-stalls/chao-muc-co-ba.jpg", 10.760529999999999, 106.69488, "Chao Muc Co Ba", "17:30-22:30", 35.0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stalls");
        }
    }
}
