using Buratino.DI;
using Buratino.Entities.Abstractions;
using Buratino.Repositories.Implementations.Postgres;
using Buratino.Repositories.RepositoryStructure;
using NHibernate;

namespace Buratino.Repositories.Implementations
{
    public class PGRepository<T> : RepositoryBase<T> where T : IEntityBase
    {
        public ISessionFactory SessionFactory { get; set; } = Container.Get<IPGSessionFactory>().SessionFactory;

        public PGRepository()
        {

        }

        public override IQueryable<T> GetAll()
        {
            var session = SessionFactory.OpenSession();
            return session.Query<T>();
        }

        public override T Get(Guid id)
        {
            using (var session = SessionFactory.OpenSession())
            {
                return session.Get<T>(id);
            }
        }

        public override T Insert(T entity)
        {
            using (var session = SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    session.Save(entity);
                    trans.Commit();
                    return entity;
                }
            }
        }

        public override T Update(T entity)
        {
            using (var session = SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    session.Update(entity);
                    trans.Commit();
                    return entity;
                }
            }
        }

        public override bool Delete(T entity)
        {
            if (entity is PersistentEntity persistent)
            {
                persistent.IsDeleted = true;
                Update(entity);
                return true;
            }
            else
            {
                return HardDelete(entity);
            }
        }

        public override bool Delete(Guid id)
        {
            return Delete(Get(id));
        }

        private bool HardDelete(T entity)
        {
            using (var session = SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    session.Delete(entity);
                    trans.Commit();
                    return true;
                }
            }
        }
    }
}