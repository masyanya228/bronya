using Buratino.API;
using Buratino.Attributes;
using Buratino.Xtensions;
using Buratino.Helpers;
using Buratino.Models.DomainService.DomainStructure;
using Bronya.Entities;
using Buratino.DI;
using Buratino.Entities;
using Bronya.Services;

namespace vkteams.Services
{
    /// <summary>
    /// Сервис взаимодействия с пользователем через telegram
    /// </summary>
    public class BronyaService : BronyaServiceBase
    {
        public IDomainService<Table> TableDS { get; set; } = Container.GetDomainService<Table>();
        public IDomainService<Book> BookDS { get; set; } = Container.GetDomainService<Book>();
        public BronyaService(LogService logService, TGAPI tgAPI) : base(logService, tgAPI)
        {
        }

        [TGPointer("start", "menu")]
        private string Com_Menu(long chatId, int messageId = 0)
        {
            return TGAPI.SendOrEdit(chatId,
                GetContactsForMenu(),
                messageId,
                new InlineKeyboardConstructor()
                    .AddButtonDown("Бронь", "/book")
                    .AddButtonDown("Правила", $"/rules")
                    .AddButtonDown("Меню", $"/barmenu")
                    .AddButtonDown("Соц. сети", $"/socmedia"));
        }

        [TGPointer("rules")]
        private string Rules(long chatId, int messageId = 0)
        {
            return TGAPI.SendOrEdit(chatId,
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Правила заведения:" +
                $"\r\n #1 трезвость" +
                $"\r\n #2 без детей" +
                $"\r\n #3 ...",
                messageId,
                new InlineKeyboardConstructor()
                    .AddButtonDown("Назад", $"/menu"));
        }

        [TGPointer("barmenu")]
        private string BarMenu(long chatId, int messageId = 0)
        {
            return TGAPI.SendOrEdit(chatId,
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n *тут будут картинки с меню*",
                messageId,
                new InlineKeyboardConstructor()
                    .AddButtonDown("Назад", $"/menu"));
        }

        [TGPointer("socmedia")]
        private string SocialMedias(long chatId, int messageId = 0)
        {
            return TGAPI.SendOrEdit(chatId,
                $"{GetContactsForMenu()}",
                messageId,
                new InlineKeyboardConstructor()
                    .AddButtonDown("Insta", "#")
                    .AddButtonDown("Telegram группа", "#")
                    .AddButtonDown("Чат с администратором", "#")
                    .AddButtonDown("Назад", $"/menu"));
        }

        [TGPointer("book")]
        private string Book(Account acc, long chatId, int messageId = 0)
        {
            if (new BookService().GetMyActualBook(acc).Any())
            {
                return MyBook(acc, chatId, messageId);
            }
            return TGAPI.SendOrEdit(chatId,
                $"{GetContactsForMenu()}",
                messageId,
                new InlineKeyboardConstructor()
                    .AddTableButtons()
                    .AddButtonDown("Назад", $"/menu"));
        }

        [TGPointer("table")]
        private string Table(Account acc, long chatId, string tableName, int messageId = 0)
        {
            var table = TableDS.GetAll().Where(x => x.Name == tableName).Single();
            acc.SelectedTable = table;
            AccountService.AccountDS.Save(acc);

            DateTime[] avalableTimes = new BookService().GetAvailableTimes(table);
            if(!avalableTimes.Any())
            {
                return TGAPI.SendOrEdit(chatId,
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Стол: {table.Name}" +
                $"\r\n *Посадочных мест: {table.NormalSeatAmount}*" +
                $"\r\n\r\n К сожалению этот стол уже нельзя забронировать. Выберите пожалуйста другой.",
                messageId,
                new InlineKeyboardConstructor()
                    .AddButtonDown("Другой стол", $"/book"));
            }

            return TGAPI.SendOrEdit(chatId,
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Стол: {table.Name}" +
                $"\r\n *Посадочных мест: {table.NormalSeatAmount}*",
                messageId,
                new InlineKeyboardConstructor()
                    .AddTimeButtons(avalableTimes)
                    .AddButtonDown("Назад", $"/book"));
        }

        [TGPointer("time")]
        private string Time(Account acc, long chatId, DateTime time, int messageId = 0)
        {
            if (acc.SelectedTable is null)
            {
                return Book(acc, chatId, messageId);
            }
            acc.SelectedTime = time;
            AccountService.AccountDS.Save(acc);

            return TGAPI.SendOrEdit(chatId,
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Стол: {acc.SelectedTable.Name}" +
                $"\r\n *Посадочных мест: {acc.SelectedTable.NormalSeatAmount}*" +
                $"\r\n На {acc.SelectedTime:dd.MM} в {acc.SelectedTime:HH:mm}",
                messageId,
                new InlineKeyboardConstructor()
                    .AddPlacesButtons(acc.SelectedTable)
                    .AddButtonDown("Назад", $"/table/{acc.SelectedTable.Name}"));
        }

        [TGPointer("places")]
        private string Places(Account acc, long chatId, int places, int messageId = 0)
        {
            if (acc.SelectedTable is null || acc.SelectedTime == default)
            {
                return Book(acc, chatId, messageId);
            }

            if (!new BookService().GetAvailableTimes(acc.SelectedTable).Contains(acc.SelectedTime))
            {
                return TGAPI.SendOrEdit(chatId,
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n Похоже кто-то успел раньше вас забронировать стол на это время. Выберите другой столик или время :)",
                    messageId,
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Другой столик", $"/book")
                        .AddButtonDown("Другое время", $"/table/{acc.SelectedTable.Name}")
                        .AddButtonDown("В главное меню", $"/menu"));
            }

            Book newBook = new Book()
            {
                Account = acc,
                ActualBookStartTime = acc.SelectedTime,
                Table = acc.SelectedTable,
                BookEndTime = acc.SelectedTime.AddHours(2),
                SeatAmount = places,
            };
            BookDS.Save(newBook);

            return MyBook(acc, chatId, messageId);
        }

        [TGPointer("mybook")]
        private string MyBook(Account acc, long chatId, int messageId)
        {
            var book = new BookService()
                .GetMyActualBook(acc)
                .OrderBy(x => x.ActualBookStartTime)
                .FirstOrDefault();

            if (book is null)
            {
                return Com_Menu(chatId, messageId);
            }

            return TGAPI.SendOrEdit(chatId,
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Вы забронировали стол: #{book.Table.Name}" +
                $"\r\n *На {book.ActualBookStartTime:dd.MM} в {book.ActualBookStartTime:HH:mm}" +
                $"\r\n Гостей: {book.SeatAmount}*",
                messageId,
                new InlineKeyboardConstructor()
                    .AddButtonDown("Отменить бронь", $"/try_cancel_book")
                    .AddButtonDown("Перенести на 20 минут", $"/try_move_book")
                    .AddButtonDown("Назад", $"/menu"));
        }

        [TGPointer("morethan")]
        private string TooManyPlaces(long chatId, int messageId = 0)
        {
            return TGAPI.SendOrEdit(chatId,
                $"Для того, чтобы забронировать стол на такое количество гостей, позвоните администратору:" +
                $"\r\n{GetContactsForMenu()}",
                messageId,
                new InlineKeyboardConstructor()
                    .AddButtonDown("Главное меню", "/menu"));
        }

        private static string GetContactsForMenu()
        {
            return "+7(927)4351814";
        }
    }
}
