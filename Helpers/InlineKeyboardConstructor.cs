using Telegram.Bot.Types.ReplyMarkups;

namespace Bronya.Helpers
{
    public class InlineKeyboardConstructor : IReplyConstructor
    {
        public List<List<InlineKeyboardButton>> KeyboardButtons { get; set; } = [];

        public InlineKeyboardConstructor()
        {
        }

        public InlineKeyboardConstructor(IEnumerable<InlineKeyboardButton> verticalStack)
        {
            foreach (var item in verticalStack)
            {
                KeyboardButtons.Add(new() { item });
            }
        }

        public InlineKeyboardConstructor(IEnumerable<IEnumerable<InlineKeyboardButton>> array)
        {
            KeyboardButtons = array.Select(x => x.ToList()).ToList();
        }

        public InlineKeyboardConstructor AddButtonRight(string title, string callbackData)
        {
            if (!KeyboardButtons.Any())
            {
                KeyboardButtons.Add([]);
            }
            KeyboardButtons.Last().Add(new InlineKeyboardButton(title) { CallbackData = callbackData });
            return this;
        }

        public InlineKeyboardConstructor AddButtonDown(string title, string callbackData)
        {
            KeyboardButtons.Add(new() { new InlineKeyboardButton(title) { CallbackData = callbackData } });
            return this;
        }

        public InlineKeyboardConstructor AddLinkDown(string title, string url)
        {
            KeyboardButtons.Add(new() { new InlineKeyboardButton(title) { Url = url } });
            return this;
        }

        public InlineKeyboardConstructor AddGrid(IEnumerable<IEnumerable<InlineKeyboardButton>> array)
        {
            KeyboardButtons.AddRange(array.Select(x => x.ToList()).ToList());
            return this;
        }

        public IReplyMarkup GetMarkup()
        {
            foreach (var row in KeyboardButtons)
            {
                foreach (var item in row)
                {
                    item.Text ??= "Пусто";
                    item.CallbackData ??= "/menu";
                }
            }
            return new InlineKeyboardMarkup(KeyboardButtons);
        }
    }
}
