using VinhKhanhGuide.Domain.Entities;

namespace VinhKhanhGuide.Infrastructure.Persistence.Seeding;

public static class StallSeedData
{
    public static IReadOnlyCollection<Stall> Stalls { get; } =
    [
        new Stall
        {
            Id = 1,
            Name = "Oc Co Lan",
            DescriptionVi = "Quan oc mau sample noi bat voi cac mon oc xao bo toi va oc huong nuong phuc vu khach du lich.",
            Latitude = 10.76042,
            Longitude = 106.69321,
            TriggerRadiusMeters = 40,
            Category = "Hai san",
            OpenHours = "16:00-23:00",
            ImageUrl = "https://example.com/sample-stalls/oc-co-lan.jpg",
            AverageRating = 4.50m
        },
        new Stall
        {
            Id = 2,
            Name = "Nuong Gio Dem",
            DescriptionVi = "Diem nuong sample tren duong Vinh Khanh voi thit nuong, hai san nuong va khong gian ngoai troi don gian.",
            Latitude = 10.76088,
            Longitude = 106.69402,
            TriggerRadiusMeters = 45,
            Category = "Do nuong",
            OpenHours = "17:00-23:30",
            ImageUrl = "https://example.com/sample-stalls/nuong-gio-dem.jpg",
            AverageRating = 4.20m
        },
        new Stall
        {
            Id = 3,
            Name = "So 1 Cua Rang Me",
            DescriptionVi = "Quan sample chuyen cua rang me, tom xoc toi va cac mon hai san dam vi phu hop de demo danh sach gian hang.",
            Latitude = 10.75997,
            Longitude = 106.69375,
            TriggerRadiusMeters = 35,
            Category = "Hai san",
            OpenHours = "15:30-22:30",
            ImageUrl = "https://example.com/sample-stalls/cua-rang-me.jpg",
            AverageRating = 4.70m
        },
        new Stall
        {
            Id = 4,
            Name = "Lau Dem Vinh Khanh",
            DescriptionVi = "Quan lau sample gia dinh voi cac mon lau hai san, lau thai va lau bo duoc tao de seed du lieu khoi dau.",
            Latitude = 10.76011,
            Longitude = 106.69451,
            TriggerRadiusMeters = 50,
            Category = "Lau",
            OpenHours = "16:30-23:00",
            ImageUrl = "https://example.com/sample-stalls/lau-dem-vinh-khanh.jpg",
            AverageRating = 4.10m
        },
        new Stall
        {
            Id = 5,
            Name = "Banh Trang Nuong 198",
            DescriptionVi = "Xe banh trang nuong sample nho gon, phuc vu mon an vat buoi toi voi trung, pho mai va kho bo.",
            Latitude = 10.76102,
            Longitude = 106.69348,
            TriggerRadiusMeters = 30,
            Category = "An vat",
            OpenHours = "18:00-22:00",
            ImageUrl = "https://example.com/sample-stalls/banh-trang-nuong-198.jpg",
            AverageRating = 4.00m
        },
        new Stall
        {
            Id = 6,
            Name = "Chao Muc Co Ba",
            DescriptionVi = "Quan sample phuc vu chao muc nong, muc chien va mon an nhe phu hop khach tham quan cuoi buoi.",
            Latitude = 10.76053,
            Longitude = 106.69488,
            TriggerRadiusMeters = 35,
            Category = "Mon nuoc",
            OpenHours = "17:30-22:30",
            ImageUrl = "https://example.com/sample-stalls/chao-muc-co-ba.jpg",
            AverageRating = 4.30m
        }
    ];
}
