using Buratino.Entities.Abstractions;

namespace Bronya.Entities
{
    public class Book : EntityBase
    {
        public virtual int SeatAmount { get; set; }

        public virtual bool IsCanceled { get; set; }

        public virtual DateTime ActualBookStartTime { get; set; }

        /// <summary>
        /// Время выноса кальяна
        /// </summary>
        public virtual DateTime TableStarted { get; set; }

        /// <summary>
        /// Продолжительность брони
        /// </summary>
        public virtual TimeSpan BookLength { get; set; }

        /// <summary>
        /// Бронь кончается
        /// </summary>
        public virtual DateTime BookEndTime
        {
            get
            {
                return ActualBookStartTime.Add(BookLength);
            }
        }
        
        /// <summary>
        /// Время расчета стола
        /// </summary>
        public virtual DateTime TableClosed { get; set; }
        
        public virtual Table Table { get; set; }
    }
}
