using NHibernate;

namespace Buratino.Repositories.Implementations.Postgres
{
    public interface IPGSessionFactory
    {
        ISessionFactory SessionFactory { get; set; }
    }
}