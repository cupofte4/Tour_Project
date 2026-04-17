namespace Tour_Project.Models
{
    public static class Roles
    {
        public const string Admin = "admin";
        public const string Manager = "manager";

        public static string Normalize(string? role)
        {
            var value = (role ?? string.Empty).Trim().ToLowerInvariant();
            return value switch
            {
                "admin" => Admin,
                "manager" => Manager,
                _ => Manager   // default fallback = manager (no user role)
            };
        }

        public static bool IsValid(string? role)
        {
            var v = (role ?? string.Empty).Trim().ToLowerInvariant();
            return v == Admin || v == Manager;
        }
    }
}
