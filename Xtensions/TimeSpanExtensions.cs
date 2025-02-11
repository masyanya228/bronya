namespace Bronya.Xtensions
{
    public static class TimeSpanExtensions
    {
        public static string ToHHmm(this TimeSpan timeSpan)
        {
            return $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}";
        }
    }
}
