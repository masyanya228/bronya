using Bronya.Entities.Abstractions;

namespace Bronya.Entities
{
    public class Table : PersistentEntity
    {
        public virtual int NormalSeatAmount { get; set; }

        public virtual bool HasConsole { get; set; }
        
        public virtual bool IsBookAvailable { get; set; }

        /// <summary>
        /// Порядковый номер стола
        /// </summary>
        public virtual int Number { get; set; }

        public override string ToString()
        {
            return HasConsole
                ? $"{Name}🎮"
                : $"{Name}";
        }

        public virtual string GetState()
        {
            var state = $"Название: {Name}" +
                $"\r\nМест: {NormalSeatAmount}";
            if (HasConsole)
                state += $"\r\nЕсть приставка 🎮";
            if (!IsBookAvailable)
                state += $"\r\n🚫Онлайн бронирование не доступно!";
            return state ;
        }

        public virtual string GetTitle()
        {
            var state = string.Empty;
            if (!IsBookAvailable)
                state += $"🚫";
            if (HasConsole)
                state += $"🎮";
             state += $"Стол: {Name}" +
                $" 👤{NormalSeatAmount}";
            return state;
        }
    }
}
