using Bronya.Entities;

using Buratino.Xtensions;

using Telegram.Bot.Types;

namespace Bronya.Dtos
{
    public class DataPackage
    {
        public DataPackage(Account acc, Update update)
        {
            Account = acc;
            Update = update;
        }

        public DataPackage(Account acc)
        {
            Account = acc;
            ChatId = Account.TGChatId.AsLong();
        }

        public Account Account { get; set; }

        public Update Update { get; set; }

        public long ChatId { get; set; }

        public int MessageId { get; set; }

        public string Command { get; set; }
    }
}