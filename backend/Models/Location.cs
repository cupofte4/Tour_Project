namespace Tour_Project.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string? Images { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? ReviewsJson { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? TextVi { get; set; }
        public string? TextEn { get; set; }
        public string? TextZh { get; set; }
        public string? TextDe { get; set; }
        public int? ManagerId { get; set; }

        // Navigation properties
        public User? Manager { get; set; }
        public ICollection<LocationReview>? Reviews { get; set; }
        public ICollection<LocationStat>? Stats { get; set; }
        public ICollection<LocationManagerAssignment>? ManagerAssignments { get; set; }
    }
}
