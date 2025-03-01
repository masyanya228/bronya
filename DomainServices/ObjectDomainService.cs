using Bronya.Entities.Abstractions;

using Buratino.Models.DomainService.DomainStructure;

using Buratino.Xtensions;

namespace Buratino.Models.DomainService
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
