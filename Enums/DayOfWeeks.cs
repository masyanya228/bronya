using Bronya.Attributes;

namespace Bronya.Enums
{
    [Flags]
    public enum DayOfWeeks
    {
        None = 0,
        
        [Title("Понедельник", "пн")]
        Monday = 1,

        [Title("Вторник", "вт")]
        Tuesday = 1 << 1,

        [Title("Среда", "ср")]
        Wednesday = 1 << 2,

        [Title("Четверг", "чт")]
        Thursday = 1 << 3,

        [Title("Пятница", "пт")]
        Friday = 1 << 4,

        [Title("Суббота", "сб")]
        Saturday = 1 << 5,

        [Title("Воскресенье", "вс")]
        Sunday = 1 << 6,

        [Title("Вся неделя")]
        AllDays = (1 << 7) - 1,
    }
}