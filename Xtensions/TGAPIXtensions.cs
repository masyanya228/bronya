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

        public static string EscapeFormat(this string source)
        {
            return EscapeMarkdown2(source);
        }

        private static string EscapeMarkdown1(this string source)
        {
            if (source == null)
                return null;
            return source.Replace("_", "\\_").Replace("*", "\\*").Replace("`", "\\`").Replace("[", "\\[");
        }

        private static string EscapeMarkdown2(this string source)
        {
            if (source == null)
                return null;

            var chars = new char[] { '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };
            foreach (var item in chars)
            {
                source = source.Replace(item.ToString(), "\\" + item);
            }
            return source;
        }
    }
}
