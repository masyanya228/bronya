using Bronya.Caching.Structure;
using Bronya.DomainServices.DomainStructure;
using Bronya.Entities;
using Bronya.Services;

using System.Linq.Expressions;

namespace Bronya.DomainServices
{
    public class TableDomainService : PersistentDomainService<Table>
    {
        public ICacheService<IEnumerable<Table>> CacheService { get; }

        public TableDomainService(ICacheService<IEnumerable<Table>> cacheService)
        {
            CacheService = cacheService;
        }

        public override IEnumerable<Table> GetAll(Expression<Func<Table, bool>> filter = null)
        {
            var str = filter?.ToString() ?? "all";
            return CacheService.Get(() => base.GetAll(filter), str);
        }

        public override Table Save(Table entity)
        {
            var book = base.Save(entity);
            CacheService.Remove(null);
            new NowMenuMessageUpdateService(Account).UpdateNowMenuMessages(AuthorizeService.Instance.TgAPI);
            return book;
        }

        public override bool Delete(Table entity)
        {
            var state = base.Delete(entity);
            CacheService.Remove(null);
            new NowMenuMessageUpdateService(Account).UpdateNowMenuMessages(AuthorizeService.Instance.TgAPI);
            return state;
        }
    }
}
