namespace Buratino.Repositories.RepositoryStructure
{
    public interface IRepository<T>
    {
        T Get(Guid id);
        IQueryable<T> GetAll();
        T Insert(T entity);
        T Update(T entity);
        bool Delete(Guid id);
    }
}