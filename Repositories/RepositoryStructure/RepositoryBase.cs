using Bronya.Dtos;
using Bronya.Entities.Abstractions;

using System.Linq.Expressions;

namespace Buratino.Repositories.RepositoryStructure
{
    public abstract class RepositoryBase<T> : IRepository<T>
        where T : IEntityBase
    {
        public abstract IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null);

        public abstract QueryableSession<T> GetAllQuery();

        public abstract T Get(Guid id);

        public abstract T Insert(T entity);

        public abstract T Update(T entity);

        public abstract bool Delete(T entity);

        public abstract bool Delete(Guid id);
    }
}