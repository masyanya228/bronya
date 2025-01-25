using Buratino.Entities.Abstractions;

namespace Buratino.Repositories.RepositoryStructure
{
    public abstract class RepositoryBase<T> : IRepository<T>
        where T : IEntityBase
    {
        public abstract IQueryable<T> GetAll();

        public abstract T Get(Guid id);

        public abstract T Insert(T entity);

        public abstract T Update(T entity);

        public abstract bool Delete(T entity);

        public abstract bool Delete(Guid id);
    }
}