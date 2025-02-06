using Buratino.Entities;
using Buratino.Entities.Abstractions;

namespace Bronya.Entities
{
    public class PermissionRoleLink : EntityBase
    {
        public virtual string Permission { get; set; }
        public virtual Role Role { get; set; }
    }
}