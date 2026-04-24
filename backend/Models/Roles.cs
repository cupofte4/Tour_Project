namespace Tour_Project.Models
{
    public static class Roles
    {
        public const string ADMIN = "admin";
        public const string MANAGER = "manager";

        public const string Admin = ADMIN;
        public const string Manager = MANAGER;

        public static string Normalize(string? role)
        {
            var value = (role ?? string.Empty).Trim().ToLowerInvariant();
            return value switch
            {
                "admin" => ADMIN,
                "manager" => MANAGER,
                _ => MANAGER   // default fallback = manager (no user role)
            };
        }

        public static bool IsValid(string? role)
        {
            var v = (role ?? string.Empty).Trim().ToLowerInvariant();
            return v == ADMIN || v == MANAGER;
        }
    }
}
