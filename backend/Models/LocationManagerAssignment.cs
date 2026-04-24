using System.ComponentModel.DataAnnotations.Schema;

namespace Tour_Project.Models
{
    public class LocationManagerAssignment
    {
        public int Id { get; set; }
        
        public int ManagerId { get; set; }
        
        public int LocationId { get; set; }

        [ForeignKey(nameof(ManagerId))]
        public AdminUser? Manager { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location? Location { get; set; }
    }
}