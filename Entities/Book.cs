using Bronya.Entities.Abstractions;
using Bronya.Enums;
using Bronya.Services;
using Bronya.Xtensions;

using Buratino.Helpers;
using Buratino.Xtensions;

using vkteams.Xtensions;

namespace Bronya.Entities
{
    public class Book : PersistentEntity
    {
        public virtual Account Guest { get; set; }

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
        
        /// <summary>
        /// Фактическая продолжительность брони после закрытия
        /// </summary>
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
        /// Время закрытия брони
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
                return TableStarted < ActualBookStartTime //todo - возможно надо переписать
                    ? TableStarted
                    : ActualBookStartTime;
            return ActualBookStartTime;
        }

        /// <summary>
        /// Возвращает конец брони
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetTrueEndBook()
        {
            if (TableClosed != default)
                return TableClosed;
            if (TableStarted != default)
                return TableAllowedStarted.Add(BookLength);
            return BookEndTime;
        }

        public virtual string GetState()
        {
            var smena = new BookService(null).Smena;

            string state = "Бронь:";
            state += $"\r\n⏱️Время: {ActualBookStartTime.ToHHmm()}";
            state += $"\r\n🔲Стол: {Table.Name.EscapeFormat()}";
            if (Table.HasConsole)
                state += "🎮";

            state += $"\r\n👤Гостей: {SeatAmount}";
            state += $"\r\nИмя: {Guest}";
            if (Guest.TGTag != default)
                state += $"\r\n@{Guest.TGTag.EscapeFormat()}";
            if (Guest.Phone != default)
                state += $"\r\n{Guest.Phone.EscapeFormat()}";

            if (IsCanceled)
            {
                state += $"\r\n*🚫Отменена🚫*";
            }
            if (TableClosed != default)
            {
                state += $"\r\n\r\n*Стол закрыт: {TableClosed.ToHHmm()}*";
            }
            else if (TableStarted != default)
            {
                var timeEnd = TableAllowedStarted.Add(BookLength);
                var timeLeft = timeEnd.Subtract(new TimeService().GetNow());

                state += "\r\n";
                if (TableAllowedStarted != TableStarted)
                    state += $"\r\n*Опоздание*";
                state += $"\r\n*Вынос кальяна: {TableStarted.ToHHmm()}; Стол до: {timeEnd.ToHHmm()}" +
                    $"\r\nОсталось: {timeLeft.TotalMinutes.Round().ToString().EscapeFormat()} мин*";
            }
            return state;
        }

        public virtual string GetEditState(Account account)
        {
            var smena = new BookService(account).Smena;

            string state = "Бронь:";
            if (ActualBookStartTime == account.SelectedTime)
                state += $"\r\n⏱️Время: {ActualBookStartTime.ToddMM_HHmm()}";
            else
                state += $"\r\n⏱️Время: ~{ActualBookStartTime.ToddMM_HHmm()}~ *{account.SelectedTime.ToddMM_HHmm()}*";

            var consoleTitle = Table.HasConsole ? "🎮" : string.Empty;
            var console2Title = account.SelectedTable.HasConsole ? "🎮" : string.Empty;
            if (Table == account.SelectedTable)
                state += $"\r\n🔲Стол: {Table.Name.EscapeFormat()}{consoleTitle}";
            else
                state += $"\r\n🔲Стол: ~{Table.Name.EscapeFormat()}{consoleTitle}~ *{account.SelectedTable.Name.EscapeFormat()}{console2Title}*";

            state += $"\r\n👤Гостей: {SeatAmount}";

            state += $"\r\nИмя: {Guest}";
            if (Guest.TGTag != default)
                state += $"\r\n@{Guest.TGTag.EscapeFormat()}";
            if (Guest.Phone != default)
                state += $"\r\n{Guest.Phone.EscapeFormat()}";

            if (IsCanceled)
            {
                state += $"\r\n*🚫Отменена🚫*";
                return state;
            }

            if (TableStarted != default)
            {
                var timeEnd = TableAllowedStarted.Add(BookLength);
                var timeLeft = timeEnd.Subtract(new TimeService().GetNow());

                state += "\r\n";
                if (TableAllowedStarted != TableStarted)
                    state += $"\r\n*Опоздание*";
                state += $"\r\n*Вынос кальяна: {TableStarted.ToHHmm()}; Стол до: {timeEnd.ToHHmm()}" +
                    $"\r\nОсталось: {timeLeft.TotalMinutes.Round()} мин*";
            }
            if (TableClosed != default)
            {
                state += $"\r\n\r\n*Стол закрыт: {TableClosed.ToHHmm()}*";
            }
            return state;
        }

        public virtual string GetTitle()
        {
            var closedTitle = TableClosed != default ? "⛔️" : "";
            return $"{closedTitle}{ActualBookStartTime.ToHHmm()} {Guest.ToString()} 👤:{SeatAmount}";
        }

        public virtual InlineKeyboardConstructor GetButtons()
        {
            if (IsCanceled)
            {
                return new InlineKeyboardConstructor()
                    .AddButtonDown("🟢", $"/try_repair/{Id}")
                    .AddButtonRight("🔲", $"/table/{Table.Id}");
            }
            else if (TableClosed != default)
            {
                return new InlineKeyboardConstructor()
                    .AddButtonDownIf(() => Guest.Phone == default, "Добавить телефон", $"/select_phone/{Guest.Id}")
                    .AddButtonDownIf(() => Guest.CardNumber == default, "Добавить карту", $"/select_card/{Guest.Id}")
                    .AddButtonDown("↔️", $"/try_prolongate/{Id}")
                    .AddButtonRight("🔲", $"/table/{Table.Id}")
                    .AddButtonDown("✏️ Гость", $"/account/{Guest.Id}");
            }
            else if (TableStarted != default)
            {
                return new InlineKeyboardConstructor()
                    .AddButtonDown("⛔️", $"/try_close/{Id}")
                    .AddButtonRight("↔️", $"/try_prolongate/{Id}")
                    .AddButtonRight("П", $"/edit/{Id}")
                    .AddButtonDown("🔲", $"/table/{Table.Id}");
            }
            else
            {
                return new InlineKeyboardConstructor()
                    .AddButtonRight("✅", $"/try_start_book/{Id}")
                    .AddButtonDown("🔴", $"/try_cancel/{Id}")
                    .AddButtonRight("⤵️", $"/try_move/{Id}")
                    .AddButtonRight("П", $"/edit/{Id}")
                    .AddButtonDown("🔲", $"/table/{Table.Id}");
            }
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
