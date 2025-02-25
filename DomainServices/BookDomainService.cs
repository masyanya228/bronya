using Bronya.Entities;
using Bronya.Services;

using Buratino.Models.DomainService.DomainStructure;

namespace Buratino.Models.DomainService
{
    public class BookDomainService : PersistentDomainService<Book>
    {
        public override Book Save(Book entity)
        {
            var book = base.Save(entity);
            new NowMenuMessageUpdateService(Account).UpdateNowMenuMessages(AuthorizeService.Instance.TgAPI);
            return book;
        }

        public override bool Delete(Book entity)
        {
            var state = base.Delete(entity);
            new NowMenuMessageUpdateService(Account).UpdateNowMenuMessages(AuthorizeService.Instance.TgAPI);
            return state;
        }
    }
}
