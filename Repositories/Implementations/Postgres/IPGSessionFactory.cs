using NHibernate;

namespace Bronya.Repositories.Implementations.Postgres
{
    public interface IPGSessionFactory
    {
        ISessionFactory SessionFactory { get; set; }
    }
}