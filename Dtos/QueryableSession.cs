namespace Bronya.Dtos
{
    public class QueryableSession<T> : IDisposable
    {
        public IQueryable<T> Query { get; set; }

        public NHibernate.ISession Session { get; set; }

        public void Dispose()
        {
            Session.Dispose();
        }
    }
}
