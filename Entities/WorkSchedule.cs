using Bronya.Entities.Abstractions;
using Bronya.Enums;
using Bronya.Xtensions;

using Buratino.Models.Attributes;
using Buratino.Xtensions;

namespace Bronya.Entities
{
    public class WorkSchedule : PersistentEntity
    {
        /// <summary>
        /// Начало смены
        /// </summary>
        public virtual TimeSpan Start { get; set; } = new TimeSpan(14, 0, 0);

        /// <summary>
        /// Конец смены (расчет)
        /// </summary>
        public virtual TimeSpan End { get => Start.Add(Length); }

        /// <summary>
        /// Продолжительность смены
        /// Кратно step
        /// </summary>
        public virtual TimeSpan Length { get; set; } = new TimeSpan(12, 0, 0);

        /// <summary>
        /// Шаг с которым бронируется время
        /// </summary>
        public virtual TimeSpan Step { get; set; } = new TimeSpan(0, 20, 0);

        /// <summary>
        /// Буфферное время между бронями
        /// Кратно step
        /// </summary>
        public virtual TimeSpan Buffer { get; set; } = new TimeSpan(0, 20, 0);

        /// <summary>
        /// Период на который гость бронирует стол
        /// Кратно step
        /// </summary>
        public virtual TimeSpan MinPeriod { get; set; } = new TimeSpan(2, 0, 0);

        /// <summary>
        /// Дата начала работы нового графика
        /// </summary>
        public virtual DateTime StartDate { get; set; }

        /// <summary>
        /// Время, после которого бронь автоматически отменится
        /// </summary>
        public virtual TimeSpan AutoCancelBook { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Время, после которого бронь автоматически отменится
        /// </summary>
        public virtual TimeSpan NotificationBeforeBookEnd { get; set; } = new TimeSpan(0, 10, 0);

        /// <summary>
        /// Это график на 1 день
        /// </summary>
        public virtual bool IsOneTimeSchedule { get; set; }
        
        /// <summary>
        /// Выходной
        /// </summary>
        public virtual bool IsDayOff { get; set; }

        public virtual DayOfWeeks DayOfWeeks { get; set; } = DayOfWeeks.AllDays;

        public virtual string GetState()
        {
            string state = "*Когда работает этот график:*\r\n";
            if (IsOneTimeSchedule)
            {
                state += $"Особый график на {StartDate.ToddMMyyyy()}";
            }
            else
            {
                state += $"График работы с {StartDate.ToddMMyyyy()}";
            }

            if (IsDayOff)
            {
                state += $"\r\n*Выходной*";
                return state;
            }

            if (DayOfWeeks == DayOfWeeks.AllDays)
            {
                state += $"\r\nВсе дни недели";
            }
            else if (!IsOneTimeSchedule && DayOfWeeks == DayOfWeeks.None)
            {
                state += $"\r\n*Не выбраны дни недели для этого графика!*";
            }
            else if (!IsOneTimeSchedule)
            {
                state += $"\r\nДни недели:\r\n";
                state += Enum.GetValues<DayOfWeeks>().Except([DayOfWeeks.None, DayOfWeeks.AllDays]).Where(x => DayOfWeeks.HasFlag(x)).Select(x => x.GetAttribute<TitleAttribute>().Description).Join(", ");
            }

            state += $"\r\n\r\n*Настройки смены:*";
            state += $"\r\nРабочие часы: {Start.ToHHmm()} - {Start.Add(Length).ToHHmm()} ({Length.TotalHours.Round(1)} ч.)";
            state += $"\r\nМинимальная бронь: {MinPeriod.ToHHmm()}";
            state += $"\r\nБуффер: {Buffer.ToHHmm()}";
            state += $"\r\nШаг бронирования: {Step.ToHHmm()}";
            state += $"\r\n\r\nАвтоматическая отмена брони через: {AutoCancelBook.ToHHmm()}";
            state += $"\r\nНапоминание о конце брони за: {NotificationBeforeBookEnd.ToHHmm()}";
            return state;
        }
    }
}
