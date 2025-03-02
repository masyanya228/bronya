using Bronya.API;
using Bronya.DI;
using Bronya.DomainServices.DomainStructure;
using Bronya.Entities;

namespace Bronya.Services
{
    public class NowMenuMessageUpdateService
    {
        public static LogToFileService LogToFileService { get; set; }
        public static LogService LogService { get; set; }
        public IDomainService<Account> AccountDS { get; set; }

        public NowMenuMessageUpdateService(Account account)
        {
            AccountDS = Container.GetDomainService<Account>(account);
            LogToFileService = Container.Get<LogToFileService>();
            LogService = new LogService(account);
        }

        public IEnumerable<Account> UpdateNowMenuMessages(TGAPI tGAPI)
        {
            var accToUpdate = AccountDS.GetAll().Where(x => x.NowMenuMessageId != default).ToArray();
            foreach (var acc in accToUpdate)
            {
                var service = new BronyaHostesService(LogToFileService, tGAPI, acc)
                {
                    Package = new Dtos.DataPackage(acc) { MessageId = acc.NowMenuMessageId }
                };
                service.Now();
                LogService.LogEvent(nameof(UpdateNowMenuMessages) + ":" + acc.Id);
            }
            return accToUpdate;
        }
    }
}
