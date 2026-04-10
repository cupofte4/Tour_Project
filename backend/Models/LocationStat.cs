namespace Tour_Project.Models
{
    public class LocationStat
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public DateTime StatDate { get; set; }
        public int ViewsCount { get; set; }
        public int AudioPlaysCount { get; set; }

        public Location? Location { get; set; }
    }
}

