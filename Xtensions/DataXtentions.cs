using Telegram.Bot.Types.ReplyMarkups;

namespace Buratino.Xtensions
{
    public static class DataXtentions
    {
        public static IEnumerable<IEnumerable<InlineKeyboardButton>> ToGrid<T>(this IEnumerable<T> filters, Func<T, string> titleSelector, Func<T, string> callbackSelector, int columnCount = 2)
        {
            return filters.Chunk(columnCount).Select(x => x.Select(y => new InlineKeyboardButton(titleSelector(y)) { CallbackData = callbackSelector(y) }));
        }
    }
}
