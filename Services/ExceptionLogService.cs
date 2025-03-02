using Bronya.DI;
using Bronya.DomainServices.DomainStructure;
using Bronya.Entities;
using Bronya.Xtensions;

using Buratino.Xtensions;

namespace Bronya.Services
{
    /// <summary>
    /// История изменений в БД, инициированных пользователем
    /// </summary>
    public class ExceptionLogService
    {
        public IDomainService<ExceptionLog> LogDS { get; set; }

        public void LogEvent(Account account, Exception exception)
        {
            string messages = exception.CollectMessagesFromException();
            string stackTraces = exception.CollectStackTracesFromException();
            LogDS = Container.GetDomainService<ExceptionLog>(account);
            LogDS.Save(new ExceptionLog()
            {
                Name = exception.GetType().AssemblyQualifiedName,
                StackTrace = stackTraces.Truncate(2000),
                Message = messages.Truncate(500)
            });
        }
    }
}
