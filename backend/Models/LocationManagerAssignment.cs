namespace Tour_Project.Models
{
    public class LocationManagerAssignment
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public int LocationId { get; set; }

        public User? Manager { get; set; }
        public Location? Location { get; set; }
    }
}

