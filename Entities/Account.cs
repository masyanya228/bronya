using Bronya.Attributes;
using Bronya.Entities.Abstractions;
using Bronya.Enums;
using Bronya.Xtensions;

namespace Bronya.Entities
{
    public class Account : PersistentEntity
    {
        public virtual string LastName { get; set; }

        [Title("Сдвиг времени", "")]
        public virtual decimal TimeOffsetInHours { get; set; }

        [Title("Последняя аутентификация")]
        public virtual DateTime LastEnter { get; set; }

        [Title("Заблокирован?")]
        public virtual bool IsBlocked { get; set; }

        [Title("Временная блокировка истекает")]
        public virtual DateTime DynamicBlockExpiring { get; set; }

        [Title("Причина блокировки")]
        public virtual string BlockReason { get; set; }

        [Title("Id чата в Telegram")]
        public virtual string TGChatId { get; set; }

        public virtual string TGTag { get; set; }

        public virtual bool IsPhoneRequested { get; set; }

        public virtual string Phone { get; set; }

        public virtual string CardNumber { get; set; }

        public virtual Table SelectedTable { get; set; }

        public virtual DateTime SelectedTime { get; set; }

        public virtual int SelectedPlaces { get; set; }

        public virtual WaitingText Waiting { get; set; }

        public virtual Account SelectedAccount { get; set; }

        /// <summary>
        /// Номер просматриваемой страницы в реестре гостей
        /// </summary>
        public virtual int GetAccountsPage { get; set; }

        public virtual IList<Role> Roles { get; set; } = [];
        public virtual WorkSchedule SelectedSchedule { get; set; }

        /// <summary>
        /// Актуальное сообщение "Сейчас"
        /// </summary>
        public virtual int NowMenuMessageId { get; set; }

        /// <summary>
        /// Бронь для редактирования
        /// </summary>
        public virtual Book SelectedBook { get; set; }

        public virtual string GetNewBookState()
        {
            string state = "Новая бронь:";
            if (SelectedTime != default)
            {
                state += $"\r\n⏱️Время: {SelectedTime.ToddMM_HHmm()}";
            }
            if (SelectedTable != default)
            {
                state += $"\r\n🔲Стол: {SelectedTable.Name.EscapeFormat()}";
                if (SelectedTable.HasConsole)
                    state += "🎮";
            }
            if (SelectedPlaces != default)
            {
                state += $"\r\n👤Гостей: {SelectedPlaces}";
            }
            return state;
        }

        public virtual bool IsFake()
        {
            return TGChatId == null;
        }

        public override string ToString()
        {
            return $"{Name.EscapeFormat()}";
        }

        /// <summary>
        /// Без mdV2
        /// </summary>
        /// <returns></returns>
        public virtual string GetCardTitle()
        {
            string title = $"{Name} {Phone} ({CardNumber})";
            if (TGTag != default)
                title = "✅" + title;
            return title;
        }

        public virtual string GetCard()
        {
            string state = $"{Name.EscapeFormat()}";
            if (CardNumber != default)
            {
                state += $"\r\nКарта: {CardNumber.EscapeFormat()}";
            }
            if (TGTag != default)
            {
                state += $"\r\n@{TGTag.EscapeFormat()}";
            }
            if (Phone != default)
            {
                state += $"\r\n{Phone.EscapeFormat()}";
            }
            return state;
        }
    }
}