using Bronya.Enums;
using Bronya.Services;

namespace Bronya.Xtensions
{
    public static class DateTimeExtensions
    {
        public static string ToddMMyyyy(this DateTime dateTime) => dateTime.Year != new TimeService().GetNow().Year
            ? $"{dateTime:dd.MM.yyyy}"
            : $"{dateTime:dd.MM}";

        public static DayOfWeeks ToDayOfWeeks(this DayOfWeek dayOfWeek) => Enum.TryParse(dayOfWeek.ToString(), out DayOfWeeks result)
            ? result
            : DayOfWeeks.None;
    }
}
