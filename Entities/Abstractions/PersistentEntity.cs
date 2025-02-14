using Buratino.Entities.Abstractions;

namespace Bronya.Entities.Abstractions
{
    public abstract class PersistentEntity : NamedEntity
    {
        public virtual DateTime DeletedStamp { get; set; }

        public virtual bool IsDeleted { get; set; }

        public virtual Account WhoDeleted { get; set; }

        public virtual DateTime UpdatedStamp { get; set; }

        public virtual Account WhoUpdated { get; set; }
    }
}