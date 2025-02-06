using Bronya.Entities.Abstractions;

namespace Buratino.Models.DomainService.DomainStructure
{
    public class PersistentDomainService<T> : DomainServiceBase<T> where T : PersistentEntity
    {
        public override IQueryable<T> GetAll()
        {
            //Остается опция для доступа к удаленным объектам
            return Repository.GetAll().Where(x => !x.IsDeleted);
        }
    }
}
