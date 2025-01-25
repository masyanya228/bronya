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
            var session = SessionFactory.OpenSession();
            return session.Query<T>().Where(x => x.IsDeleted == false);
        }
    }
}