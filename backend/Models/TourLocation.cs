namespace Tour_Project.Models
{
    public class TourLocation
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public int LocationId { get; set; }
        public int OrderIndex { get; set; }
        public bool IsOptional { get; set; } = false;

        public Tour? Tour { get; set; }
        public Location? Location { get; set; }
    }
}
