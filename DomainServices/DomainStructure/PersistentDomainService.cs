using Buratino.Entities.Abstractions;

namespace Buratino.Models.DomainService.DomainStructure
{
    public abstract class PersistentDomainService<T> : DomainServiceBase<T> where T : PersistentEntity
    {
        public override IQueryable<T> GetAll()
        {
            return Repository.GetAll().Where(x => !x.IsDeleted);
        }
    }
}
