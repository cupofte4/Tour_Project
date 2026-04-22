namespace Tour_Project.Models
{
    public class LocationManagerAssignment
    {
        public int Id { get; set; }
        // Reference to AdminUsers table (manager is an admin)
        public int AdminUserId { get; set; }
        public int LocationId { get; set; }

        public AdminUser? Manager { get; set; }
        public Location? Location { get; set; }
    }
}

