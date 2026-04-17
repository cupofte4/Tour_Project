using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VinhKhanhGuide.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationMetadataToStalls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Stalls",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Stalls",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Stalls",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Stalls",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Stalls",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Stalls",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Stalls",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrlsJson",
                table: "Stalls",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Stalls",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReviewsJson",
                table: "Stalls",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "ImageUrlsJson",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "ReviewsJson",
                table: "Stalls");

            migrationBuilder.InsertData(
                table: "Stalls",
                columns: new[] { "Id", "AudioUrl", "AverageRating", "Category", "DescriptionVi", "ImageUrl", "IsActive", "Latitude", "Longitude", "MapLink", "Name", "NarrationScriptVi", "OpenHours", "Priority", "TriggerRadiusMeters" },
                values: new object[,]
                {
                    { 1, "https://example.com/sample-audio/oc-co-lan.mp3", 4.50m, "Hai san", "Quan oc mau sample noi bat voi cac mon oc xao bo toi va oc huong nuong phuc vu khach du lich.", "https://example.com/sample-stalls/oc-co-lan.jpg", true, 10.76042, 106.69320999999999, "https://maps.apple.com/?ll=10.76042,106.69321&q=Oc%20Co%20Lan", "Oc Co Lan", "Ban dang den gan Oc Co Lan, mot diem dung chan sample noi bat voi cac mon oc xao bo toi va oc huong nuong tren duong Vinh Khanh.", "16:00-23:00", 100, 40.0 },
                    { 2, "https://example.com/sample-audio/nuong-gio-dem.mp3", 4.20m, "Do nuong", "Diem nuong sample tren duong Vinh Khanh voi thit nuong, hai san nuong va khong gian ngoai troi don gian.", "https://example.com/sample-stalls/nuong-gio-dem.jpg", true, 10.76088, 106.69401999999999, "https://maps.apple.com/?ll=10.76088,106.69402&q=Nuong%20Gio%20Dem", "Nuong Gio Dem", "Nuong Gio Dem la diem POI sample phu hop cho narration ve khong gian an toi ngoai troi va cac mon nuong pho bien cua khu vuc.", "17:00-23:30", 90, 45.0 },
                    { 3, "https://example.com/sample-audio/cua-rang-me.mp3", 4.70m, "Hai san", "Quan sample chuyen cua rang me, tom xoc toi va cac mon hai san dam vi phu hop de demo danh sach gian hang.", "https://example.com/sample-stalls/cua-rang-me.jpg", true, 10.759969999999999, 106.69374999999999, "https://maps.apple.com/?ll=10.75997,106.69375&q=So%201%20Cua%20Rang%20Me", "So 1 Cua Rang Me", "So 1 Cua Rang Me duoc seed nhu mot diem ke chuyen sample cho cac mon hai san dam vi va nhiep song buoi toi tren tuyen pho am thuc.", "15:30-22:30", 80, 35.0 },
                    { 4, "https://example.com/sample-audio/lau-dem-vinh-khanh.mp3", 4.10m, "Lau", "Quan lau sample gia dinh voi cac mon lau hai san, lau thai va lau bo duoc tao de seed du lieu khoi dau.", "https://example.com/sample-stalls/lau-dem-vinh-khanh.jpg", true, 10.760109999999999, 106.69450999999999, "https://maps.apple.com/?ll=10.76011,106.69451&q=Lau%20Dem%20Vinh%20Khanh", "Lau Dem Vinh Khanh", "Lau Dem Vinh Khanh la diem narration sample cho nhom du khach muon dung chan lau hon va tim mon lau hai san vao buoi toi.", "16:30-23:00", 70, 50.0 },
                    { 5, "https://example.com/sample-audio/banh-trang-nuong-198.mp3", 4.00m, "An vat", "Xe banh trang nuong sample nho gon, phuc vu mon an vat buoi toi voi trung, pho mai va kho bo.", "https://example.com/sample-stalls/banh-trang-nuong-198.jpg", true, 10.76102, 106.69347999999999, "https://maps.apple.com/?ll=10.76102,106.69348&q=Banh%20Trang%20Nuong%20198", "Banh Trang Nuong 198", "Banh Trang Nuong 198 dai dien cho nhom diem POI an vat sample, huu ich khi app muon kich hoat narration ngan o cu ly gan.", "18:00-22:00", 60, 30.0 },
                    { 6, "https://example.com/sample-audio/chao-muc-co-ba.mp3", 4.30m, "Mon nuoc", "Quan sample phuc vu chao muc nong, muc chien va mon an nhe phu hop khach tham quan cuoi buoi.", "https://example.com/sample-stalls/chao-muc-co-ba.jpg", true, 10.760529999999999, 106.69488, "https://maps.apple.com/?ll=10.76053,106.69488&q=Chao%20Muc%20Co%20Ba", "Chao Muc Co Ba", "Chao Muc Co Ba duoc giu lam POI sample cho nhung diem dung cuoi hanh trinh, noi du khach co the nghe tom tat va tim mon an nhe.", "17:30-22:30", 50, 35.0 }
                });
        }
    }
}
