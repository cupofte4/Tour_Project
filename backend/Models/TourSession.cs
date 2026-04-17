namespace Tour_Project.Models
{
    public class TourSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TourId { get; set; }
        public string LanguageCode { get; set; } = "vi";
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public Tour? Tour { get; set; }
        public User? User { get; set; }
        public ICollection<SessionVisit> Visits { get; set; } = [];
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
