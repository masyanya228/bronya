using Telegram.Bot.Types.ReplyMarkups;

namespace Buratino.Helpers
{
    public class InlineKeyboardConstructor
    {
        public List<List<InlineKeyboardButton>> KeyboardButtons { get; set; } = new();

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
                KeyboardButtons.Add(new());
            }
            KeyboardButtons.Last().Add(new InlineKeyboardButton(title) { CallbackData = callbackData });
            return this;
        }

        public InlineKeyboardConstructor AddButtonDown(string title, string callbackData)
        {
            KeyboardButtons.Add(new() { new InlineKeyboardButton(title) { CallbackData = callbackData } });
            return this;
        }

        public InlineKeyboardConstructor AddGrid(IEnumerable<IEnumerable<InlineKeyboardButton>> array)
        {
            KeyboardButtons.AddRange(array.Select(x => x.ToList()).ToList());
            return this;
        }

        public InlineKeyboardMarkup GetMarkup()
        {
            foreach (var row in KeyboardButtons)
            {
                foreach (var item in row)
                {
                    if (item.Text is null)
                        item.Text = "Пусто";
                    if (item.CallbackData is null)
                        item.CallbackData = "/menu";
                }
            }
            return new InlineKeyboardMarkup(KeyboardButtons);
        }
    }
}
