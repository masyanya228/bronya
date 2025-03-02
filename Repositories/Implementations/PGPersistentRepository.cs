using Bronya.Entities.Abstractions;
using Bronya.Repositories.Implementations;
using Bronya.Services;

using Buratino.Repositories.RepositoryStructure;

using System.Linq.Expressions;

namespace Buratino.Repositories.Implementations
{
    /// <summary>
    /// Использовать для репозиториев, в которых нужно ограничить разработчика от удаленых объектов
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PGPersistentRepository<T> : PGRepository<T> where T : PersistentEntity, IEntityBase
    {
        public PGPersistentRepository()
        {

        }

        public override IEnumerable<T> GetAll(Expression<Func<T, bool>> filter)
        {
            //if (filter != null)
            //{
            //    Expression<Func<T, bool>> notDeleted = x => x.IsDeleted == false;
            //    var notDeletedC = notDeleted.Compile();
            //    var filterC = filter.Compile();
            //    filter = c => notDeletedC(c) && filterC(c);
            //}
            return base.GetAll(filter).Where(x => !x.IsDeleted);
        }

        public override bool Delete(T entity)
        {
            entity.IsDeleted = true;
            entity.DeletedStamp = new TimeService().GetNow();
            return base.Update(entity) != default;
        }

        public override bool Delete(Guid guid)
        {
            return Delete(base.Get(guid));
        }
    }
}