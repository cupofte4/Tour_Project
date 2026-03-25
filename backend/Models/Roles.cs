namespace Tour_Project.Models
{
    public static class Roles
    {
        public const string Admin = "admin";
        public const string User = "user";

        public static string Normalize(string? role)
        {
            var value = (role ?? string.Empty).Trim().ToLowerInvariant();
            return value switch
            {
                "admin" => Admin,
                "user" => User,
                _ => User
            };
        }
    }
}

