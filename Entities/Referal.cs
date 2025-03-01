using Bronya.Entities.Abstractions;

namespace Bronya.Entities
{
    public class Referal : PersistentEntity
    {
        /// <summary>
        /// Кто привел
        /// </summary>
        public virtual Account Main { get; set; }

        /// <summary>
        /// Кого привел
        /// </summary>
        public virtual Account NewSub { get; set; }

        public virtual bool IsPaid { get; set; }
    }
}