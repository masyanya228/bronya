﻿using Bronya.Entities;

using Buratino.Entities.Abstractions;
using Buratino.Enums;
using Buratino.Models.Attributes;

namespace Buratino.Entities
{
    public class Account : PersistentEntity
    {
        public virtual string LastName { get; set; }

        [TitleDescribe("Сдвиг времени", "")]
        public virtual decimal TimeOffsetInHours { get; set; }

        [TitleDescribe("Последняя аутентификация")]
        public virtual DateTime LastEnter { get; set; }

        [TitleDescribe("Заблокирован?")]
        public virtual bool IsBlocked { get; set; }

        [TitleDescribe("Временная блокировка истекает")]
        public virtual DateTime DynamicBlockExpiring { get; set; }

        [TitleDescribe("Причина блокировки")]
        public virtual string BlockReason { get; set; }

        [TitleDescribe("Id чата в Telegram")]
        public virtual string TGChatId { get; set; }
        
        public virtual string TGTag { get; set; }

        public virtual Table SelectedTable { get; set; }

        public virtual DateTime SelectedTime { get; set; }

        public virtual int SelectedPlaces { get; set; }

        public virtual WaitingText Waiting { get; set; }

        public virtual string GetNewBookState()
        {
            string state = "Новая бронь:";
            if (SelectedTime != default)
            {
                state += $"\r\n⏱️Время: {SelectedTime:dd.MM HH:mm}";
            }
            if (SelectedTable != default)
            {
                state += $"\r\n🔲Стол: {SelectedTable.Name}";
            }
            if (SelectedPlaces != default)
            {
                state += $"\r\n👤Гостей: {SelectedPlaces}";
            }
            return state;
        }

        public override string ToString()
        {
            return $"{Name} {LastName}";
        }
    }
}