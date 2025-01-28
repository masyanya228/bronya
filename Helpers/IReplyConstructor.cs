
using Telegram.Bot.Types.ReplyMarkups;

namespace Buratino.Helpers
{
    public interface IReplyConstructor
    {
        public IReplyMarkup GetMarkup();
    }
}