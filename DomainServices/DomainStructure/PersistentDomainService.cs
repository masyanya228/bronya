using Bronya.Entities.Abstractions;

using System.Linq.Expressions;

namespace Buratino.Models.DomainService.DomainStructure
{
    public class PersistentDomainService<T> : DomainServiceBase<T> where T : PersistentEntity
    {
        public override IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null)
        {
            //todo сделать компиляцию фильтра
            //Остается опция для доступа к удаленным объектам
            return Repository.GetAll(filter).Where(x => !x.IsDeleted);
        }
    }
}
