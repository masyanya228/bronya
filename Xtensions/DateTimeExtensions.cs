using Bronya.Enums;
using Bronya.Services;

using static System.Runtime.InteropServices.JavaScript.JSType;
using System;

namespace Bronya.Xtensions
{
    public static class DateTimeExtensions
    {
        public static string ToHHmm(this DateTime dateTime)
        {
            return dateTime.TimeOfDay.ToHHmm();
        }

        public static string ToddMM(this DateTime dateTime)
        {
            if (dateTime.Year == new TimeService().GetNow().Year)
            {
                return $"{dateTime:dd.MM}".EscapeFormat();
            }
            else
            {
                return $"{dateTime:dd.MM.yyyy}".EscapeFormat();
            }
        }

        public static string ToddMM_HHmm(this DateTime dateTime)
        {
            return dateTime.ToddMM() + " " + dateTime.ToHHmm();
        }

        public static DayOfWeeks ToDayOfWeeks(this DayOfWeek dayOfWeek) => Enum.TryParse(dayOfWeek.ToString(), out DayOfWeeks result)
            ? result
            : DayOfWeeks.None;

        public static DateTime Round(this DateTime dateTime, TimeSpan epsilon)
        {
            long ticks = dateTime.Ticks / epsilon.Ticks;
            return new DateTime(ticks * epsilon.Ticks, dateTime.Kind);
        }
    }
}
