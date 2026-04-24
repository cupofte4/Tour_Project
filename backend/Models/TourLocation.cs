using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tour_Project.Models
{
    [Index(nameof(TourId), nameof(LocationId))]
    public class TourLocation
    {
        public int Id { get; set; }

        public int TourId { get; set; }
        public Tour? Tour { get; set; }

        public int LocationId { get; set; }
        public Location? Location { get; set; }

        public int OrderIndex { get; set; }
        public bool IsOptional { get; set; } = false;
    }
}
