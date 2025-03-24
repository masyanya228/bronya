using Bronya.Caching.Structure;
using Bronya.Entities;

namespace Bronya.Caching
{
    public class RelevantGuestCacheService : InMemoryCacheService<IEnumerable<Account>>
    {
        public RelevantGuestCacheService()
        {
            TimeExpiration = TimeSpan.FromHours(2);
        }

        public override object Remove(IEnumerable<Account> accounts)
        {
            foreach (var item in Cache)
            {
                Cache.Remove(item.Key);
            }
            return null;
        }

        public override string GetKey(IEnumerable<Account> accounts)
        {
            throw new NotImplementedException();
        }
    }
}
