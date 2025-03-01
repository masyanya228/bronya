using Bronya.Dtos;
using Bronya.Entities;

namespace Bronya.Caching.Structure
{
    public static class CacheRegistrator
    {
        public static void RegisterCaches(IServiceCollection services)
        {
            services.AddSingleton(typeof(ICacheService<Account>), new AccountCacheService());

            services.AddSingleton(typeof(ICacheService<SmenaDto>), new SmenaCacheService());

            services.AddSingleton(typeof(ICacheService<IEnumerable<Table>>), new TableCacheService());

            services.AddSingleton(typeof(ICacheService<StreamFileIdDto>), new FileIdCacheService());
        }
    }
}
