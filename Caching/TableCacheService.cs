using Bronya.Caching.Structure;
using Bronya.Entities;

namespace Bronya.Caching
{
    public class TableCacheService : InMemoryCacheService<IEnumerable<Table>>
    {
        public TableCacheService()
        {
            TimeExpiration = TimeSpan.FromMinutes(15);
        }

        public override object Remove(IEnumerable<Table> tables)
        {
            foreach (var item in Cache)
            {
                Cache.Remove(item.Key);
            }
            return null;
        }

        public override string GetKey(IEnumerable<Table> entity)
        {
            throw new NotImplementedException();
        }
    }
}
