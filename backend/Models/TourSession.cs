using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tour_Project.Models
{
    [Index(nameof(TourId))]
    public class TourSession
    {
        public int Id { get; set; }

        // For guest-only system use DeviceId (string) instead of UserId
        [Required, MaxLength(200)]
        public string DeviceId { get; set; } = string.Empty;

        public int TourId { get; set; }
        public string LanguageCode { get; set; } = "vi";
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public Tour? Tour { get; set; }
        public ICollection<SessionVisit> Visits { get; set; } = new List<SessionVisit>();
    }

    public class SessionVisit
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int LocationId { get; set; }
        public bool AudioPlayed { get; set; } = false;
        public DateTime VisitedAt { get; set; } = DateTime.UtcNow;

        public TourSession? Session { get; set; }
        public Location? Location { get; set; }
    }
}
