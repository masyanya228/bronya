using Bronya.Dtos;

using System.Linq.Expressions;

namespace Bronya.Repositories.RepositoryStructure
{
    public interface IRepository<T>
    {
        T Get(Guid id);
        IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null);
        QueryableSession<T> GetAllQuery();
        T Insert(T entity);
        T Update(T entity);
        bool Delete(Guid id);
    }
}