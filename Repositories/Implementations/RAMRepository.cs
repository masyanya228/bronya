using System.Collections.Concurrent;
using Buratino.Entities.Abstractions;
using FluentNHibernate.Data;
using Buratino.Repositories.RepositoryStructure;

namespace Buratino.Repositories.Implementations
{
    public class RAMRepository<T> : RepositoryBase<T> where T : IEntityBase
    {
        public ConcurrentDictionary<Guid, T> Collection;

        public RAMRepository()
        {
            Collection = new ConcurrentDictionary<Guid, T>();
        }

        public override bool Delete(T entity) => Delete(entity.Id);

        public override bool Delete(Guid id) => Collection.TryRemove(id, out _);

        public override T Get(Guid id) => Collection.TryGetValue(id, out T value)
            ? value
            : throw new Exception("Не получилось добавить элемент в репозиторий");

        public override IQueryable<T> GetAll() => Collection.Values.AsQueryable();

        public override T Insert(T entity)
        {
            var newId = GetNextKey();
            entity.Id = newId;
            return Collection.TryAdd(newId, entity)
                ? entity
                : throw new Exception("Не получилось добавить элемент в репозиторий");
        }

        public override T Update(T entity)
        {
            Collection[entity.Id] = entity;
            return entity;
        }
        private Guid GetNextKey() => Guid.NewGuid();
    }
}
