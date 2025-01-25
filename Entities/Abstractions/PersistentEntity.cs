namespace Buratino.Entities.Abstractions
{
    public abstract class PersistentEntity : NamedEntity
    {
        public virtual DateTime DeletedStamp { get; set; }

        public virtual bool IsDeleted { get; set; }

        public virtual Account Account { get; set; }
    }
}