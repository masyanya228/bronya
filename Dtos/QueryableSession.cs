namespace Bronya.Dtos
{
    public class QueryableSession<T>
    {
        public IQueryable<T> Query { get; set; }

        public NHibernate.ISession Session { get; set; }
    }
}
