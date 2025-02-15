using Bronya.Entities.Abstractions;
using Bronya.Enums;
using Bronya.Services;

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

        public virtual DateTime TableAllowedStarted { get; set; }

        /// <summary>
        /// Продолжительность брони
        /// </summary>
        public virtual TimeSpan BookLength { get; set; }
        
        public virtual TimeSpan FactBookLength
        {
            get
            {
                if (TableClosed != default)
                    return TableClosed.Subtract(TableStarted);
                else
                    return BookLength;
            }
        }

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
            if (TableStarted != default)
                return TableStarted.Add(BookLength);
            return BookEndTime;
        }

        public virtual string GetState()
        {
            var smena = new BookService(null).GetCurrentSmena();

            string state = "Бронь:";
            state += $"\r\n⏱️Время: {ActualBookStartTime:dd.MM HH:mm}";
            state += $"\r\n🔲Стол: {Table.Name.EscapeMarkdown1()}";
            if (Table.HasConsole)
                state += "🎮";

            state += $"\r\n👤Гостей: {SeatAmount}";
            state += $"\r\nИмя: {Account}";
            if (Account.TGTag != default)
                state += $"\r\n@{Account.TGTag.EscapeMarkdown1()}";
            if (Account.Phone != default)
                state += $"\r\n{Account.Phone.EscapeMarkdown1()}";

            if (IsCanceled)
            {
                state += $"\r\n*🚫Отменена🚫*";
            }
            if (TableClosed != default)
            {
                state += $"\r\n\r\n*Стол закрыт: {TableClosed:HH:mm}*";
            }
            else if (TableStarted != default)
            {
                var timeEnd = BookEndTime.Add(smena.Schedule.Buffer);
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
