namespace Bronya.Services
{
    public class TimeService
    {
        public static bool IsDebug = false;
        public DateTime GetNow() => IsDebug
            ? new DateTime(2025, 2, 16, 17, 0, 0)
            : DateTime.Now;
    }
}
