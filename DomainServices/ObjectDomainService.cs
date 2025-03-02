using Bronya.DomainServices.DomainStructure;
using Bronya.Entities.Abstractions;

using Buratino.Xtensions;

namespace Bronya.DomainServices
{
    public class ObjectDomainService : DomainServiceBase<IEntityBase>
    {
        public ObjectDomainService()
        {
        }

        public ObjectDomainService(object domainService)
        {
            DomainService = domainService;
        }

        public object DomainService { get; set; }
        public IQueryable<IEntityBase> GetAllEntities()
        {
            var res = DomainService.InvokeMethod("GetAll");
            return res as IQueryable<IEntityBase>;
        }
    }
}
