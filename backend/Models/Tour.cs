using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tour_Project.Models
{
    public class Tour
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? CoverImage { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TourLocation> TourLocations { get; set; } = new List<TourLocation>();
    }
}
