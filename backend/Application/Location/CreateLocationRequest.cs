namespace backend.Application.Location
{
    public class CreateLocationRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public int? Prio { get; set; }
    }
}
