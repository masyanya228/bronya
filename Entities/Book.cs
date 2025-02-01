using Bronya.Enums;
using Bronya.Services;

using Buratino.Entities.Abstractions;
using Buratino.Xtensions;

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

        public virtual DateTime NotifiedAboutEndBook { get; set; }

        /// <summary>
        /// Время расчета стола
        /// </summary>
        public virtual DateTime TableClosed { get; set; }

        public virtual Table Table { get; set; }

        /// <summary>
        /// Комментарии
        /// </summary>
        public virtual string Comment { get; set; }

        public virtual DateTime GetTrueStartBook()
        {
            if (TableStarted != default)
                return TableStarted < ActualBookStartTime
                    ? TableStarted
                    : ActualBookStartTime;
            return ActualBookStartTime;
        }

        /// <summary>
        /// Возвращает
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetTrueEndBook()
        {
            var smena = new BookService().GetCurrentSmena();
            if (TableStarted != default)
                return TableStarted.Add(BookLength);
            return BookEndTime;
        }

        public virtual string GetState()
        {
            var smena = new BookService().GetCurrentSmena();
            string state = "Бронь:";
            state += $"\r\n⏱️Время: {ActualBookStartTime:dd.MM HH:mm}";
            state += $"\r\n🔲Стол: {Table.Name}";
            state += $"\r\n👤Гостей: {SeatAmount}";
            state += $"\r\nИмя: {Account.ToString()}";

            if (IsCanceled)
            {
                state += $"*🚫Отменена🚫*";
            }
            if (TableClosed != default)
            {
                state += $"\r\n\r\n*Стол закрыт: {TableClosed:HH:mm}*";
            }
            else if (TableStarted != default)
            {
                var allowedStart = ActualBookStartTime.Add(smena.Schedule.Buffer) < TableStarted
                    ? ActualBookStartTime.Add(smena.Schedule.Buffer)
                    : TableStarted;
                var timeEnd = allowedStart.Add(BookLength);
                var timeLeft = timeEnd.Subtract(new TimeService().GetNow());
                state += $"\r\n\r\n*Вынос кальяна: {TableStarted:HH:mm}; Стол до: {timeEnd:HH:mm}" +
                    $"\r\nОсталось: {timeLeft.TotalMinutes.Round()} мин.*";
            }
            return state;
        }

        public virtual bool IsIntersected(DateTime start, DateTime end)
        {
            return DataXtensions.IsXcrossing(GetTrueStartBook(), GetTrueEndBook(), start, end);
        }

        public virtual void SetNewBookEndTime(DateTime newEndTime)
        {
            var diff = newEndTime.Subtract(BookEndTime);
            BookLength = BookLength.Add(diff);
        }

        public virtual BookStatus GetStatus()
        {
            if (IsCanceled)
                return BookStatus.Canceled;
            if (TableClosed != default)
                return BookStatus.Closed;
            if (TableStarted != default)
                return BookStatus.Opened;
            else
                return BookStatus.Booked;
        }
    }
}
