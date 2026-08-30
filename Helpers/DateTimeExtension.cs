namespace EquityHarbour.Helpers
{
    public static class DateTimeExtensions
    {
        public static string ToRelativeString(this DateTime utcDateTime)
        {
            var span = DateTime.UtcNow - utcDateTime;

            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} hour{((int)span.TotalHours == 1 ? "" : "s")} ago";
            if (span.TotalDays < 2) return $"Yesterday, {utcDateTime:hh:mm tt}";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays} days ago";

            return utcDateTime.ToString("MMM d, yyyy");
        }
    }
}