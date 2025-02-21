using Bronya.Dtos;
using Bronya.Entities;

using Buratino.Entities.Abstractions;
using Buratino.Repositories.RepositoryStructure;

using System.Linq.Expressions;

namespace Buratino.Models.DomainService.DomainStructure
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