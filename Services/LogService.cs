using Bronya.DI;
using Bronya.DomainServices.DomainStructure;
using Bronya.Entities;

namespace Bronya.Services
{
    /// <summary>
    /// История изменений в БД, инициированных пользователем
    /// </summary>
    public class LogService
    {
        public IDomainService<Log> LogDS { get; set; }

        public LogService(Account account)
        {
            LogDS = Container.GetDomainService<Log>(account);
        }

        public void LogEvent(string eventTitle)
        {
            LogDS.Save(new Log() { Name = eventTitle });
        }
    }
}
