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
        public BronyaService(LogService logService, TGAPI tGAPI) : base(logService, tGAPI)
        {
        }

        [TGPointer("start", "menu")]
        private string Com_Menu(long chatId, int messageId = 0)
        {
            return TGAPI.SendOrEdit(chatId,
                GetContactsForMenu(),
                messageId,
                new InlineKeyboardConstructor()
                    .AddButtonDown("Бронь", "/mybooklist")
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
            return TGAPI.SendOrEdit(chatId,
                $"{GetContactsForMenu()}",
                messageId,
                new InlineKeyboardConstructor()
                    .AddTableButtons()
                    .AddButtonDown("Назад", $"/mybooklist"));
        }

        [TGPointer("table")]
        private string Table(Account acc, long chatId, string tableName, int messageId = 0)
        {
            var table = TableDS.GetAll().Where(x => x.Name == tableName).Single();
            acc.SelectedTable = table;
            AccountService.AccountDS.Save(acc);

            DateTime[] avalableTimes = new BookService().GetAvailableTimesForBook(table);
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

            if (!new BookService().GetAvailableTimesForBook(acc.SelectedTable).Contains(acc.SelectedTime))
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
                BookLength = new BookService().GetCurrentSmena().Schedule.MinPeriod,
                SeatAmount = places,
            };
            BookDS.Save(newBook);

            return MyBook(newBook, chatId, messageId);
        }

        [TGPointer("mybook")]
        private string MyBook(Book book, long chatId, int messageId)
        {
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
                    .AddButtonDownIf(() => new BookService().CanMove(book), "Перенести на 20 минут", $"/try_move_book/{book.Id}")
                    .AddButtonDown("Отменить бронь", $"/try_cancel_book/{book.Id}")
                    .AddButtonDown("Назад", $"/mybooklist"));
        }

        [TGPointer("try_cancel_book")]
        private string TryCancelBook(Book book, long chatId, int messageId)
        {
            if (book is null)
            {
                return Com_Menu(chatId, messageId);
            }

            if (!new BookService().CanCancel(book))
            {
                return TGAPI.SendOrEdit(chatId,
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n *Бронь не получится отменить*",
                    messageId,
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/mybooklist"));
            }
            return TGAPI.SendOrEdit(chatId,
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Отменить бронь на {book.ActualBookStartTime:dd.MM HH:mm}?",
                messageId,
                new InlineKeyboardConstructor()
                    .AddButtonDown("Отменить", $"/cancel_book/{book.Id}")
                    .AddButtonDown("Назад", $"/mybook/{book.Id}"));
        }

        [TGPointer("cancel_book")]
        private string CancelBook(Book book, long chatId, int messageId)
        {
            if (book is null)
            {
                return Com_Menu(chatId, messageId);
            }

            if (new BookService().Cancel(book))
            {
                return MyBookList(book.Account, chatId, messageId);
            }
            else
            {
                return TGAPI.SendOrEdit(chatId,
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n *Бронь не получилось перенести*",
                    messageId,
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/mybook/{book.Id}"));
            }
        }
        
        [TGPointer("try_move_book")]
        private string TryMoveBook(Book book, long chatId, int messageId)
        {
            if (book is null)
            {
                return Com_Menu(chatId, messageId);
            }

            if (!new BookService().CanMove(book))
            {
                return TGAPI.SendOrEdit(chatId,
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n *Бронь не получится перенести*",
                    messageId,
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/mybook/{book.Id}"));
            }
            var newTime = new BookService().GetTimeAfterMove(book);
            return TGAPI.SendOrEdit(chatId,
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Перенести бронь на {newTime:dd.MM HH:mm}?",
                messageId,
                new InlineKeyboardConstructor()
                    .AddButtonDown("Перенести", $"/move_book/{book.Id}")
                    .AddButtonDown("Назад", $"/mybook/{book.Id}"));
        }

        [TGPointer("move_book")]
        private string MoveBook(Book book, long chatId, int messageId)
        {
            if (book is null)
            {
                return Com_Menu(chatId, messageId);
            }

            if (new BookService().Move(book))
            {
                return MyBook(book, chatId, messageId);
            }
            else
            {
                return TGAPI.SendOrEdit(chatId,
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n *Бронь не получилось перенести*",
                    messageId,
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/mybook/{book.Id}"));
            }
        }

        [TGPointer("mybooklist")]
        private string MyBookList(Account acc, long chatId, int messageId)
        {
            var books = new BookService()
                .GetMyActualBook(acc)
                .OrderBy(x => x.ActualBookStartTime)
                .ToArray();
            if (!books.Any())
            {
                return Book(acc, chatId, messageId);
            }

            return TGAPI.SendOrEdit(chatId,
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Ваши брони:",
                messageId,
                new InlineKeyboardConstructor()
                    .AddButtonDown("+Новая бронь", $"/book")
                    .AddBooksButtons(books)
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
