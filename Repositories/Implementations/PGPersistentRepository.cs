using Bronya.Entities.Abstractions;

using Buratino.Entities.Abstractions;

namespace Buratino.Repositories.Implementations
{
    public class PGPersistentRepository<T> : PGRepository<T> where T : PersistentEntity, IEntityBase
    {
        public PGPersistentRepository()
        {

        }

        public override IQueryable<T> GetAll()
        {
            //НЕ остается опции для доступа к удаленным объектам
            var session = SessionFactory.OpenSession();
            return session.Query<T>().Where(x => x.IsDeleted == false);
        }
    }
}