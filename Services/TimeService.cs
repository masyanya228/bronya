﻿namespace Bronya.Services
{
    public class TimeService
    {
        public static bool IsDebug = false;
        public DateTime GetNow() => IsDebug
            ? new DateTime(2025, 1, 26, 12, 0, 0)
            : DateTime.Now;
    }
}
