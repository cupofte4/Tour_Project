namespace Tour_Project.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string? Images { get; set; }
        public string Audio { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? TextVi { get; set; }
        public string? TextEn { get; set; }
        public string? TextZh { get; set; }
        public string? TextDe { get; set; }
    }
}