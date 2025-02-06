using Buratino.Entities.Abstractions;

namespace Bronya.Entities
{
    public class Table : NamedEntity
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
    }
}
