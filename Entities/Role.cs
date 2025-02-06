using Buratino.Entities.Abstractions;

namespace Bronya.Entities
{
    public class Role : NamedEntity
    {
        public virtual string Title { get; set; }
    }
}