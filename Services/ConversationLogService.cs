using Bronya.Entities;

using Buratino.DI;
using Buratino.Models.DomainService.DomainStructure;

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
            ConversationLogDS.Save(new ConversationLog() { Name = update });
        }
    }
}
