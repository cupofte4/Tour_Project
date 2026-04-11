namespace Tour_Project.Models
{
    public class LocationReview
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string Author { get; set; } = "Guest";
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Location? Location { get; set; }
    }
}
