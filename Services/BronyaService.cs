using Buratino.API;
using Buratino.Attributes;
using Buratino.Xtensions;
using Buratino.Helpers;
using Bronya.Entities;
using Bronya.Xtensions;

namespace Bronya.Services
{
    /// <summary>
    /// Сервис взаимодействия с пользователем через telegram
    /// </summary>
    public class BronyaService : BronyaServiceBase
    {
        public BronyaService(LogToFileService logService, TGAPI tGAPI, Account account) : base(logService, tGAPI, account)
        {
        }

        [ApiPointer("start", "menu")]
        private string Menu()
        {
            if (!Package.Account.IsPhoneRequested)
            {
                return AskPhone();
            }
            return SendOrEdit(
                GetContactsForMenu(),
                new InlineKeyboardConstructor()
                    .AddButtonDown("Бронь", "/mybooklist")
                    .AddButtonDown("Правила", $"/rules")
                    .AddButtonDown("Меню", $"/barmenu")
                    .AddButtonDown("Соц. сети", $"/socmedia")
                    .AddButtonDownIf(() => Package.Account.Id == new Guid("4be29f89-f887-48a1-a8af-cad15d032758"), "Роль", "/show_role")
            );
        }

        private string AskPhone()
        {
            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\nПожалуйста, поделитесь своим номером телефона, чтобы бронировать столы:".EscapeFormat(),
                new ReplyMarkupConstructor().
                    AddButtonDown("Поделиться телефоном", true)
            );
        }

        [ApiPointer("set_phone")]
        private string SetPhone(string phone)
        {
            Package.Account.Phone = AccountService.ParseNumber(phone);
            Package.Account.IsPhoneRequested = true;
            AccountService.AccountDS.Save(Package.Account);
            SendOrEdit("Спасибо! Теперь вы можете забронировать стол.".EscapeFormat());
            return Menu();
        }

        [ApiPointer("rules")]
        private string Rules()
        {
            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Правила заведения:" +
                $"\r\n 1 трезвость" +
                $"\r\n 2 без детей" +
                $"\r\n 3 ",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Назад", $"/menu"));
        }

        [ApiPointer("barmenu")]
        private string BarMenu()
        {
            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n *тут будут картинки с меню*",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Назад", $"/menu"));
        }

        [ApiPointer("socmedia")]
        private string SocialMedias()
        {
            return SendOrEdit(
                $"{GetContactsForMenu()}",
                new InlineKeyboardConstructor()
                    .AddLinkDown("Insta", "https://www.instagram.com/greenplace_kzn")
                    .AddLinkDown("Threads", "https://www.threads.net/@greenplace_kzn")
                    .AddLinkDown("Telegram группа", "https://t.me/the_green_place")
                    .AddLinkDown("Чат с администратором", "https://t.me/+S8emunypWyo4NGNi")
                    .AddButtonDown("Назад", $"/menu"));
        }

        [ApiPointer("book")]
        private string Book()
        {
            var books = BookService
                .GetMyActualBooks(Package.Account)
                .OrderBy(x => x.ActualBookStartTime)
                .ToArray();
            if (books.Any())
            {
                return SendOrEdit(
                    $"{GetContactsForMenu()}" +
                    $"\r\nУ вас уже есть бронь. Чтобы забронировать еще, позвоните по телефону.".EscapeFormat(),
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/menu")
                );
            }
            return SendOrEdit(
                $"{GetContactsForMenu()}",
                new InlineKeyboardConstructor()
                    .AddTableButtons(Package.Account)
                    .AddButtonDown("Назад", $"/menu"),
                null,
                ImageId
            );
        }

        [ApiPointer("table")]
        private string Table(string tableName)
        {
            var table = BookService.TableDS.GetAll().Where(x => x.Name == tableName).Single();
            Package.Account.SelectedTable = table;
            AccountService.AccountDS.Save(Package.Account);

            DateTime[] avalableTimes = BookService.GetAvailableTimesForBook(table, Package.Account);
            if (!avalableTimes.Any())
            {
                return SendOrEdit(
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n Стол: {table}" +
                    $"\r\n *Посадочных мест: {table.NormalSeatAmount}*" +
                    $"\r\n\r\n К сожалению этот стол уже нельзя забронировать. Выберите пожалуйста другой.".EscapeFormat(),
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Другой стол", $"/book"),
                    null,
                    ImageId
                );
            }

            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Стол: {table}" +
                $"\r\n *Посадочных мест: {table.NormalSeatAmount}*",
                new InlineKeyboardConstructor()
                    .AddTimeButtons(avalableTimes)
                    .AddButtonDown("Назад", $"/book"),
                null,
                ImageId
            );
        }

        [ApiPointer("set_time")]
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
                $"\r\n\r\n Стол: {Package.Account.SelectedTable}" +
                $"\r\n *Посадочных мест: {Package.Account.SelectedTable.NormalSeatAmount}*" +
                $"\r\n На {Package.Account.SelectedTime.ToddMM_HHmm()}",
                new InlineKeyboardConstructor()
                    .AddPlacesButtons(Package.Account.SelectedTable)
                    .AddButtonDown("Назад", $"/table/{Package.Account.SelectedTable.Name}"));
        }

        [ApiPointer("places")]
        private string Places(int places)
        {
            if (Package.Account.SelectedTable is null || Package.Account.SelectedTime == default)
            {
                return Book();
            }

            if (!BookService.GetAvailableTimesForBook(Package.Account.SelectedTable, Package.Account).Contains(Package.Account.SelectedTime))
            {
                return SendOrEdit(
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n Похоже кто-то успел раньше вас забронировать стол на это время. Выберите другой столик или время :)".EscapeFormat(),
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Другой столик", $"/book")
                        .AddButtonDown("Другое время", $"/table/{Package.Account.SelectedTable.Name}")
                        .AddButtonDown("В главное меню", $"/menu"));
            }

            Book newBook = new Book()
            {
                Guest = Package.Account,
                ActualBookStartTime = Package.Account.SelectedTime,
                Table = Package.Account.SelectedTable,
                BookLength = BookService.Smena.Schedule.MinPeriod,
                SeatAmount = places,
            };
            BookService.BookDS.Save(newBook);

            return MyBook(newBook);
        }

        [ApiPointer("mybook")]
        private string MyBook(Book book)
        {
            if (book is null)
            {
                return Menu();
            }

            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Вы забронировали стол: {book.Table}" +
                $"\r\n *На {book.ActualBookStartTime.ToddMM_HHmm()}" +
                $"\r\n Гостей: {book.SeatAmount}*",
                new InlineKeyboardConstructor()
                    .AddButtonDownIf(() => BookService.CanMove(book, Package.Account), "Перенести на 20 минут", $"/try_move_book/{book.Id}")
                    .AddButtonDown("Отменить бронь", $"/try_cancel/{book.Id}")
                    .AddButtonDown("Назад", $"/mybooklist"),
                null,
                ImageId
            );
        }

        [ApiPointer("try_cancel")]
        private string TryCancel(Book book)
        {
            if (book is null)
            {
                return Menu();
            }

            if (!BookService.CanCancel(book))
            {
                return SendOrEdit(
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n *Бронь не получится отменить*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/mybooklist"));
            }
            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n *Отменить бронь на {book.ActualBookStartTime.ToddMM_HHmm()}?*",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Отменить", $"/cancel/{book.Id}")
                    .AddButtonDown("Назад", $"/mybook/{book.Id}"));
        }

        [ApiPointer("cancel")]
        private string Cancel(Book book)
        {
            if (book is null)
            {
                return Menu();
            }

            if (BookService.Cancel(book))
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

        [ApiPointer("try_move_book")]
        private string TryMoveBook(Book book)
        {
            if (book is null)
            {
                return Menu();
            }

            if (!BookService.CanMove(book, Package.Account))
            {
                return SendOrEdit(
                    $"{GetContactsForMenu()}" +
                    $"\r\n\r\n *Бронь не получится перенести*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/mybook/{book.Id}"));
            }
            var newTime = BookService.GetTimeAfterMove(book);
            return SendOrEdit(
                $"{GetContactsForMenu()}" +
                $"\r\n\r\n Перенести бронь на {newTime.ToddMM_HHmm()}?",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Перенести", $"/move_book/{book.Id}")
                    .AddButtonDown("Назад", $"/mybook/{book.Id}"));
        }

        [ApiPointer("move_book")]
        private string MoveBook(Book book)
        {
            if (book is null)
            {
                return Menu();
            }

            if (BookService.Move(book, Package.Account))
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

        [ApiPointer("mybooklist")]
        private string MyBookList()
        {
            var books = BookService
                .GetMyActualBooks(Package.Account)
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

        [ApiPointer("morethan")]
        private string TooManyPlaces()
        {
            return SendOrEdit(
                $"Для того, чтобы забронировать стол на такое количество гостей, позвоните администратору:".EscapeFormat() +
                $"\r\n{GetContactsForMenu()}",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Главное меню", "/menu"));
        }

        private static string GetContactsForMenu()
        {
            return "+7(992)076-17-47".EscapeFormat();
        }
    }
}
