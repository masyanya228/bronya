namespace Bronya.Services
{
    public class TimeService
    {
        public static bool IsDebug = false;
        public DateTime GetNow() => IsDebug
            ? new DateTime(2025, 2, 12, 15, 0, 0)
            : DateTime.Now;
    }
}
