using Buratino.Entities.Abstractions;

namespace Buratino.Entities
{
    public class RoleAccountLink : EntityBase
    {
        public virtual Role Role { get; set; }
        public virtual Account Account { get; set; }
    }
}