using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Tour_Project.Models
{
    [Index(nameof(Name))]
    public class Location
    {
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Image { get; set; } = string.Empty;

    public string? Images { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? ReviewsJson { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [Required, MaxLength(20)]
    public string Prio { get; set; } = LocationPriority.DefaultPrio;

    public string? TextVi { get; set; }
    public string? TextEn { get; set; }
    public string? TextZh { get; set; }
    public string? TextDe { get; set; }
    }
}
