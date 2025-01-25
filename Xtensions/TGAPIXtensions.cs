using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Buratino.Xtensions
{
    public static class TGAPIXtensions
    {
        public static Task SendOrUpdateMessage(this ITelegramBotClient client, long chatId, string text, int messageId = default, InlineKeyboardMarkup inlineKeyboardMarkup = null)
        {
            if (messageId > 0)
            {
                return client.EditMessageTextAsync(chatId, messageId, text, null, null, null, inlineKeyboardMarkup);
            }
            else
            {
                return client.SendTextMessageAsync(chatId, text, null, null, null, null, null, null, null, inlineKeyboardMarkup);
            }
        }
    }
}
