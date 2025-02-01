using Buratino.Entities.Abstractions;

namespace Bronya.Entities
{
    public class WorkSchedule : NamedEntity
    {
        /// <summary>
        /// Начало смены
        /// </summary>
        public virtual TimeSpan Start { get; set; } = new TimeSpan(14, 0, 0);

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
    }
}
