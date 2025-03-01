using Bronya.Caching.Structure;
using Bronya.Entities;

namespace Bronya.Caching
{
    public class AccountCacheService : InMemoryCacheService<Account>
    {
        public override object Remove(Account entity)
        {
            return Cache.Remove(GetKey(entity));
        }

        public override string GetKey(Account entity)
        {
            return entity.TGChatId;
        }
    }
}
