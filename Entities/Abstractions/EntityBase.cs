using Bronya.Entities;

namespace Buratino.Entities.Abstractions
{
    public abstract class EntityBase : IEquatable<EntityBase>, IEntityBase
    {
        public virtual Guid Id { get; set; }

        public virtual Account Account { get; set; }

        public virtual DateTime TimeStamp { get; set; } = DateTime.Now;

        public virtual bool Equals(EntityBase other) =>
            other is not null
            && other.GetType() == GetType()
            && other.Id == Id;

        public override bool Equals(object obj) => obj is EntityBase invest && Equals(invest);

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(EntityBase a, EntityBase b) => a?.Equals(b) ?? (a is null && b is null);

        public static bool operator !=(EntityBase a, EntityBase b) => !a?.Equals(b) ?? (a is null ^ b is null);
    }
}