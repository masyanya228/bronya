namespace Bronya.Entities.Abstractions
{
    public abstract class NamedEntity : EntityBase
    {
        public virtual string Name { get; set; }
    }
}