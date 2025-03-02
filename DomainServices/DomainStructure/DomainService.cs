using Bronya.Entities.Abstractions;

namespace Bronya.DomainServices.DomainStructure
{
    public class DomainService<T> : DomainServiceBase<T> where T : IEntityBase
    {
    }
}
