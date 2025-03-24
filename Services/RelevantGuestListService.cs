using Bronya.Caching.Structure;
using Bronya.DI;
using Bronya.DomainServices.DomainStructure;
using Bronya.Entities;

namespace Bronya.Services
{
    public class RelevantGuestListService
    {
        public IDomainService<Book> BookDS { get; set; }
        public RelevantGuestListService()
        {
            BookDS = Container.GetDomainService<Book>(null);
        }

        public IEnumerable<Account> GetAccountsList()
        {
            return Container.Get<ICacheService<IEnumerable<Account>>>().Get(CountAccountList, "RelevantGuestForSetName");
        }

        private IEnumerable<Account> CountAccountList()
        {
            var startPoint = new TimeService().GetNow().AddMonths(-1);
            var booksByAccs = BookDS.GetAll(x => x.TimeStamp > startPoint)
                .GroupBy(x => x.Guest.Id)
                .Where(x => !x.First().Guest.Roles.Any())
                .OrderByDescending(x => x.Count());

            return booksByAccs.Select(x => x.First().Guest).ToArray();
        }
    }
}
