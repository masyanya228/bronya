
using Telegram.Bot.Types.ReplyMarkups;

namespace Bronya.Helpers
{
    public interface IReplyConstructor
    {
        public IReplyMarkup GetMarkup();
    }
}