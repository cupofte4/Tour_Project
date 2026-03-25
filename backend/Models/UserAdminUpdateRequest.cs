namespace Tour_Project.Models
{
    public class UserAdminUpdateRequest
    {
        public string? Password { get; set; }
        public bool IsLocked { get; set; }
    }
}
