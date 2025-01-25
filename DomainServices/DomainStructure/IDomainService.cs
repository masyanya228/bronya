using Buratino.Entities.Abstractions;

namespace Buratino.Models.DomainService.DomainStructure
{
    public interface IDomainService<T> where T : IEntityBase
    {
        IQueryable<T> GetAll();

        T Get(Guid id);

        T Save(T entity);

        T CascadeSave(T entity);
        
        bool Delete(T entity);

        bool Delete(Guid id);
    }
}