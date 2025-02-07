using Bronya.Entities.Abstractions;

using Buratino.Entities.Abstractions;

using System.Linq.Expressions;

namespace Buratino.Repositories.Implementations
{
    public class PGPersistentRepository<T> : PGRepository<T> where T : PersistentEntity, IEntityBase
    {
        public PGPersistentRepository()
        {

        }

        public override IEnumerable<T> GetAll(Expression<Func<T, bool>> filter)
        {
            Expression<Func<T, bool>> predicate = x => x.IsDeleted == false;
            var prefix = predicate.Compile();
            predicate = c => prefix(c) && filter.Compile().Invoke(c);
            return base.GetAll(predicate);
        }
    }
}