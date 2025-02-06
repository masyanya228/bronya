using Buratino.Entities.Abstractions;

namespace Bronya.Entities
{
    public class RoleAccountLink : EntityBase
    {
        public virtual Role Role { get; set; }
    }
}