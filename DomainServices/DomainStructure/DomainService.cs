using Bronya.Entities.Abstractions;

using Buratino.Models.DomainService.DomainStructure;

namespace Buratino.Models.DomainService
{
    public class DomainService<T> : DomainServiceBase<T> where T : IEntityBase
    {
    }
}
