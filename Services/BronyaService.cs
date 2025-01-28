using Buratino.API;
using Buratino.Attributes;
using Buratino.Xtensions;
using Buratino.Helpers;
using Buratino.Models.DomainService.DomainStructure;
using Bronya.Entities;
using Buratino.DI;
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
        private string Menu()
        {
            return SendOrEdit(
                GetContactsForMenu(),
                new InlineKeyboardConstructor()
                    .AddButtonDown("Бронь", "/mybooklist")
                    .AddButtonDown("Правила", $"/rules")
                    .AddButtonDown("Меню", $"/barmenu")
                    .AddButtonDown("Соц. сети", $"/socmedia"));
        }

        [TGPointer("rules")]
        private string Rules()
        {
            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Правила заведения:" +
                $"\r\n #1 трезвость" +
                $"\r\n #2 без детей" +
                $"\r\n #3 ...",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Назад", $"/menu"));
        }

        [TGPointer("barmenu")]
        private string BarMenu()
        {
            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n *тут будут картинки с меню*",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Назад", $"/menu"));
        }

        [TGPointer("socmedia")]
        private string SocialMedias()
        {
            return SendOrEdit(
                $"{GetContactsForMenu()}",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Insta", "#")
                    .AddButtonDown("Telegram группа", "#")
                    .AddButtonDown("Чат с администратором", "#")
                    .AddButtonDown("Назад", $"/menu"));
        }

        [TGPointer("book")]
        private string Book()
        {
            return SendOrEdit(
                $"{GetContactsForMenu()}",
                new InlineKeyboardConstructor()
                    .AddTableButtons()
                    .AddButtonDown("Назад", $"/mybooklist"));
        }

        [TGPointer("table")]
        private string Table(string tableName)
        {
            var table = TableDS.GetAll().Where(x => x.Name == tableName).Single();
            Package.Account.SelectedTable = table;
            AccountService.AccountDS.Save(Package.Account);

            DateTime[] avalableTimes = new BookService().GetAvailableTimesForBook(table);
            if(!avalableTimes.Any())
            {
                return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Стол: {table.Name}" +
                $"\r\n *Посадочных мест: {table.NormalSeatAmount}*" +
                $"\r\n\r\n К сожалению этот стол уже нельзя забронировать. Выберите пожалуйста другой.",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Другой стол", $"/book"));
            }

            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Стол: {table.Name}" +
                $"\r\n *Посадочных мест: {table.NormalSeatAmount}*",
                new InlineKeyboardConstructor()
                    .AddTimeButtons(avalableTimes)
                    .AddButtonDown("Назад", $"/book"));
        }

        [TGPointer("set_time")]
        private string SetTime(DateTime time)
        {
            if (Package.Account.SelectedTable is null)
            {
                return Book();
            }
            Package.Account.SelectedTime = time;
            AccountService.AccountDS.Save(Package.Account);

            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Стол: {Package.Account.SelectedTable.Name}" +
                $"\r\n *Посадочных мест: {Package.Account.SelectedTable.NormalSeatAmount}*" +
                $"\r\n На {Package.Account.SelectedTime:dd.MM} в {Package.Account.SelectedTime:HH:mm}",
                new InlineKeyboardConstructor()
                    .AddPlacesButtons(Package.Account.SelectedTable)
                    .AddButtonDown("Назад", $"/table/{Package.Account.SelectedTable.Name}"));
        }

        [TGPointer("places")]
        private string Places(int places)
        {
            if (Package.Account.SelectedTable is null || Package.Account.SelectedTime == default)
            {
                return Book();
            }

            if (!new BookService().GetAvailableTimesForBook(Package.Account.SelectedTable).Contains(Package.Account.SelectedTime))
            {
                return SendOrEdit(
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n Похоже кто-то успел раньше вас забронировать стол на это время. Выберите другой столик или время :)",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Другой столик", $"/book")
                        .AddButtonDown("Другое время", $"/table/{Package.Account.SelectedTable.Name}")
                        .AddButtonDown("В главное меню", $"/menu"));
            }

            Book newBook = new Book()
            {
                Account = Package.Account,
                ActualBookStartTime = Package.Account.SelectedTime,
                Table = Package.Account.SelectedTable,
                BookLength = new BookService().GetCurrentSmena().Schedule.MinPeriod,
                SeatAmount = places,
            };
            BookDS.Save(newBook);

            return MyBook(newBook);
        }

        [TGPointer("mybook")]
        private string MyBook(Book book)
        {
            if (book is null)
            {
                return Menu();
            }

            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Вы забронировали стол: #{book.Table.Name}" +
                $"\r\n *На {book.ActualBookStartTime:dd.MM} в {book.ActualBookStartTime:HH:mm}" +
                $"\r\n Гостей: {book.SeatAmount}*",
                new InlineKeyboardConstructor()
                    .AddButtonDownIf(() => new BookService().CanMove(book), "Перенести на 20 минут", $"/try_move_book/{book.Id}")
                    .AddButtonDown("Отменить бронь", $"/try_cancel_book/{book.Id}")
                    .AddButtonDown("Назад", $"/mybooklist"));
        }

        [TGPointer("try_cancel_book")]
        private string TryCancelBook(Book book)
        {
            if (book is null)
            {
                return Menu();
            }

            if (!new BookService().CanCancel(book))
            {
                return SendOrEdit(
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n *Бронь не получится отменить*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/mybooklist"));
            }
            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n *Отменить бронь на {book.ActualBookStartTime:dd.MM HH:mm}?*",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Отменить", $"/cancel_book/{book.Id}")
                    .AddButtonDown("Назад", $"/mybook/{book.Id}"));
        }

        [TGPointer("cancel_book")]
        private string CancelBook(Book book)
        {
            if (book is null)
            {
                return Menu();
            }

            if (new BookService().Cancel(book))
            {
                return MyBookList();
            }
            else
            {
                return SendOrEdit(
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n *Бронь не получилось перенести*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/mybook/{book.Id}"));
            }
        }
        
        [TGPointer("try_move_book")]
        private string TryMoveBook(Book book)
        {
            if (book is null)
            {
                return Menu();
            }

            if (!new BookService().CanMove(book))
            {
                return SendOrEdit(
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n *Бронь не получится перенести*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/mybook/{book.Id}"));
            }
            var newTime = new BookService().GetTimeAfterMove(book);
            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Перенести бронь на {newTime:dd.MM HH:mm}?",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Перенести", $"/move_book/{book.Id}")
                    .AddButtonDown("Назад", $"/mybook/{book.Id}"));
        }

        [TGPointer("move_book")]
        private string MoveBook(Book book)
        {
            if (book is null)
            {
                return Menu();
            }

            if (new BookService().Move(book))
            {
                return MyBook(book);
            }
            else
            {
                return SendOrEdit(
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n *Бронь не получилось перенести*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/mybook/{book.Id}"));
            }
        }

        [TGPointer("mybooklist")]
        private string MyBookList()
        {
            var books = new BookService()
                .GetMyActualBook(Package.Account)
                .OrderBy(x => x.ActualBookStartTime)
                .ToArray();
            if (!books.Any())
            {
                return Book();
            }

            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Ваши брони:",
                new InlineKeyboardConstructor()
                    .AddButtonDown("+Новая бронь", $"/book")
                    .AddBooksButtons(books)
                    .AddButtonDown("Назад", $"/menu"));
        }

        [TGPointer("morethan")]
        private string TooManyPlaces()
        {
            return SendOrEdit(
                $"Для того, чтобы забронировать стол на такое количество гостей, позвоните администратору:" +
                $"\r\n{GetContactsForMenu()}",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Главное меню", "/menu"));
        }

        private static string GetContactsForMenu()
        {
            return "+7(927)4351814";
        }
    }
}
