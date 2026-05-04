namespace Tour_Project.Models
{
    public static class LocationPriority
    {
        public const string DefaultPrio = "Silver";

        public static bool IsValid(string? prio)
        {
            return prio is "Premium" or "Gold" or "Silver";
        }

        public static string NormalizeOrDefault(string? prio)
        {
            return IsValid(prio) ? prio! : DefaultPrio;
        }

        public static int GetPrioRank(string? prio)
        {
            return prio switch
            {
                "Premium" => 3,
                "Gold" => 2,
                "Silver" => 1,
                _ => 0
            };
        }
    }
}
