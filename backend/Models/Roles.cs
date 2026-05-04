namespace Tour_Project.Models
{
    public static class Roles
    {
        public const string Admin = "admin";
        public const string Manager = "manager";

        public static bool IsValid(string? role)
        {
            return role == Admin || role == Manager;
        }
    }
}