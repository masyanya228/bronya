using Bronya.Caching.Structure;
using Bronya.DomainServices.DomainStructure;
using Bronya.Entities;

namespace Bronya.DomainServices
{
    public class AccountDomainService : PersistentDomainService<Account>
    {
        public ICacheService<Account> CacheService { get; }

        public AccountDomainService(ICacheService<Account> cacheService)
        {
            CacheService = cacheService;
        }

        public override Account Save(Account entity)
        {
            var account = base.Save(entity);
            CacheService.Remove(account);
            return account;
        }

        public override bool Delete(Account entity)
        {
            var account = base.Delete(entity);
            CacheService.Remove(entity);
            return account;
        }
    }
}
