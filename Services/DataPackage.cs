using Buratino.Entities;

using Telegram.Bot.Types;

namespace vkteams.Services
{
    public class DataPackage
    {
        public DataPackage(Account acc, Update update)
        {
            Account = acc;
            Update = update;
        }

        public Account Account { get; set; }

        public Update Update { get; set; }

        public long ChatId { get; set; }

        public int MessageId { get; set; }
    }
}