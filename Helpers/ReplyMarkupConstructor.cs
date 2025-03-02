using Telegram.Bot.Types.ReplyMarkups;

namespace Bronya.Helpers
{
    public class ReplyMarkupConstructor : IReplyConstructor
    {
        public List<List<KeyboardButton>> KeyboardButtons { get; set; } = [];

        public ReplyMarkupConstructor()
        {
        }

        public ReplyMarkupConstructor AddButtonRight(string title, bool requestContact = false)
        {
            if (!KeyboardButtons.Any())
            {
                KeyboardButtons.Add(new());
            }
            KeyboardButtons.Last().Add(new KeyboardButton(title) { RequestContact = requestContact });
            return this;
        }

        public ReplyMarkupConstructor AddButtonDown(string title, bool requestContact = false)
        {
            KeyboardButtons.Add(new() { new KeyboardButton(title) { RequestContact = requestContact } });
            return this;
        }

        public IReplyMarkup GetMarkup()
        {
            foreach (var row in KeyboardButtons)
            {
                foreach (var item in row)
                {
                    item.Text ??= "Пусто";
                }
            }
            return new ReplyKeyboardMarkup(KeyboardButtons);
        }
    }
}
