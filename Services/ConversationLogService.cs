using Bronya.DI;
using Bronya.DomainServices.DomainStructure;
using Bronya.Entities;

namespace Bronya.Services
{
    /// <summary>
    /// История команд, полученных ботом
    /// </summary>
    public class ConversationLogService
    {
        public IDomainService<ConversationLog> ConversationLogDS { get; set; }

        public ConversationLogService(Account account)
        {
            ConversationLogDS = Container.GetDomainService<ConversationLog>(account);
        }

        public void LogEvent(string update)
        {
            ConversationLogDS.Save(new ConversationLog() { Name = update.Substring(0, Math.Min(update.Length, 255)) });
        }
    }
}
