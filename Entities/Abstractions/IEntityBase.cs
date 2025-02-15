
using Bronya.Entities;

namespace Buratino.Entities.Abstractions
{
    public interface IEntityBase
    {
        Guid Id { get; set; }
        DateTime TimeStamp { get; set; }
        Account Account { get; set; }
    }
}