using Bronya.Dtos;
using Bronya.Entities;
using Bronya.Entities.Abstractions;
using Bronya.Repositories.RepositoryStructure;

using System.Linq.Expressions;

namespace Bronya.DomainServices.DomainStructure
{
    public interface IDomainService<T> where T : IEntityBase
    {
        IRepository<T> Repository { get; set; }

        Account Account { get; set; }

        IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null);

        QueryableSession<T> GetAllQuery();

        T Get(Guid id);

        T Save(T entity);

        T CascadeSave(T entity);

        bool Delete(T entity);

        bool Delete(Guid id);
    }
}