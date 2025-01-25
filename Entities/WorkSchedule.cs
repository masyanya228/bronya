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
        /// </summary>
        public virtual TimeSpan Length { get; set; } = new TimeSpan(12, 0, 0);

        /// <summary>
        /// Шаг с которым бронируется время
        /// </summary>
        public virtual TimeSpan Step { get; set; } = new TimeSpan(0, 20, 0);

        /// <summary>
        /// Буфферное время между бронями
        /// </summary>
        public virtual TimeSpan Buffer { get; set; } = new TimeSpan(0, 20, 0);

        /// <summary>
        /// Период на который гость бронирует стол
        /// </summary>
        public virtual TimeSpan MinPeriod { get; set; } = new TimeSpan(2, 0, 0);

        /// <summary>
        /// Дата начала работы нового графика
        /// </summary>
        public virtual DateTime StartDate { get; set; }

        /// <summary>
        /// Это график на 1 день
        /// </summary>
        public virtual bool IsOneTimeSchedule { get; set; }
    }
}
