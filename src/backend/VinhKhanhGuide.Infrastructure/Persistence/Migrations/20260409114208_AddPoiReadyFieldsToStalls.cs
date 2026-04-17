using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhKhanhGuide.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPoiReadyFieldsToStalls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "Stalls",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Stalls",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "MapLink",
                table: "Stalls",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NarrationScriptVi",
                table: "Stalls",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Stalls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Stalls",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AudioUrl", "IsActive", "MapLink", "NarrationScriptVi", "Priority" },
                values: new object[] { "https://example.com/sample-audio/oc-co-lan.mp3", true, "https://maps.apple.com/?ll=10.76042,106.69321&q=Oc%20Co%20Lan", "Ban dang den gan Oc Co Lan, mot diem dung chan sample noi bat voi cac mon oc xao bo toi va oc huong nuong tren duong Vinh Khanh.", 100 });

            migrationBuilder.UpdateData(
                table: "Stalls",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AudioUrl", "IsActive", "MapLink", "NarrationScriptVi", "Priority" },
                values: new object[] { "https://example.com/sample-audio/nuong-gio-dem.mp3", true, "https://maps.apple.com/?ll=10.76088,106.69402&q=Nuong%20Gio%20Dem", "Nuong Gio Dem la diem POI sample phu hop cho narration ve khong gian an toi ngoai troi va cac mon nuong pho bien cua khu vuc.", 90 });

            migrationBuilder.UpdateData(
                table: "Stalls",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AudioUrl", "IsActive", "MapLink", "NarrationScriptVi", "Priority" },
                values: new object[] { "https://example.com/sample-audio/cua-rang-me.mp3", true, "https://maps.apple.com/?ll=10.75997,106.69375&q=So%201%20Cua%20Rang%20Me", "So 1 Cua Rang Me duoc seed nhu mot diem ke chuyen sample cho cac mon hai san dam vi va nhiep song buoi toi tren tuyen pho am thuc.", 80 });

            migrationBuilder.UpdateData(
                table: "Stalls",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AudioUrl", "IsActive", "MapLink", "NarrationScriptVi", "Priority" },
                values: new object[] { "https://example.com/sample-audio/lau-dem-vinh-khanh.mp3", true, "https://maps.apple.com/?ll=10.76011,106.69451&q=Lau%20Dem%20Vinh%20Khanh", "Lau Dem Vinh Khanh la diem narration sample cho nhom du khach muon dung chan lau hon va tim mon lau hai san vao buoi toi.", 70 });

            migrationBuilder.UpdateData(
                table: "Stalls",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AudioUrl", "IsActive", "MapLink", "NarrationScriptVi", "Priority" },
                values: new object[] { "https://example.com/sample-audio/banh-trang-nuong-198.mp3", true, "https://maps.apple.com/?ll=10.76102,106.69348&q=Banh%20Trang%20Nuong%20198", "Banh Trang Nuong 198 dai dien cho nhom diem POI an vat sample, huu ich khi app muon kich hoat narration ngan o cu ly gan.", 60 });

            migrationBuilder.UpdateData(
                table: "Stalls",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AudioUrl", "IsActive", "MapLink", "NarrationScriptVi", "Priority" },
                values: new object[] { "https://example.com/sample-audio/chao-muc-co-ba.mp3", true, "https://maps.apple.com/?ll=10.76053,106.69488&q=Chao%20Muc%20Co%20Ba", "Chao Muc Co Ba duoc giu lam POI sample cho nhung diem dung cuoi hanh trinh, noi du khach co the nghe tom tat va tim mon an nhe.", 50 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "MapLink",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "NarrationScriptVi",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Stalls");
        }
    }
}
