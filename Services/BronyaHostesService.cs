using Buratino.API;
using Buratino.Attributes;
using Buratino.Xtensions;
using Buratino.Helpers;
using Buratino.Models.DomainService.DomainStructure;
using Bronya.Entities;
using Buratino.DI;
using Bronya.Services;
using Buratino.Entities;
using Buratino.Enums;

namespace vkteams.Services
{
    /// <summary>
    /// Сервис взаимодействия с пользователем через telegram
    /// </summary>
    public class BronyaHostesService : BronyaServiceBase
    {
        public IDomainService<Table> TableDS { get; set; } = Container.GetDomainService<Table>();
        public IDomainService<Book> BookDS { get; set; } = Container.GetDomainService<Book>();
        public BookService BookService { get; set; } = new BookService();

        public BronyaHostesService(LogService logService, TGAPI tGAPI) : base(logService, tGAPI)
        {
        }

        [ApiPointer("start", "menu")]
        private string Menu()
        {
            return SendOrEdit(
                "Меню хостеса:",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Сейчас", "/now")
                    .AddButtonDown("🔲 Столы", "/tables")
                    .AddButtonDown("➕📋 Новая бронь", "/book_select_time")
                );
        }

        [ApiPointer("tables")]
        private string Tables()
        {
            return SendOrEdit(
                "Меню хостеса:",
                new InlineKeyboardConstructor()
                    .AddHostesAllTableButtons()
                    .AddButtonDown("Назад", $"/menu"),
                default,
                "AgACAgIAAxkBAAOgZ5pWHbT-EVNXc96-Q0oD7LZCnGMAAjzqMRtkA9FI7XDK_OV9DSQBAAMCAAN4AAM2BA"
                );
        }

        [ApiPointer("now")]
        private string Now()
        {
            return SendOrEdit(
                "Меню хостеса:",
                new InlineKeyboardConstructor()
                    .AddHostesNowTableButtons()
                    .AddButtonDown("Назад", $"/menu"),
                default,
                "AgACAgIAAxkBAAOgZ5pWHbT-EVNXc96-Q0oD7LZCnGMAAjzqMRtkA9FI7XDK_OV9DSQBAAMCAAN4AAM2BA"
                );
        }

        [ApiPointer("table")]
        private string Table(Table table)
        {
            if (Package.Account.SelectedTable != default)
            {
                Package.Account.SelectedTable = default;
                AccountService.AccountDS.Save(Package.Account);
            }
            var books = BookService.GetCurrentBooks(table);
            var isAvailable = !table.IsBookAvailable ? "\r\n🚫 Бронь отключена" : string.Empty;
            return SendOrEdit(
                $"Стол: {table.Name}{isAvailable}",
                new InlineKeyboardConstructor()
                    .AddHostesBooksButtons(books)
                    .AddButtonDownIf(() => table.IsBookAvailable, "🚫 Отключить бронирование", $"/disable/{table.Id}")
                    .AddButtonDownIf(() => !table.IsBookAvailable, "✅ Включить бронирование", $"/enable/{table.Id}")
                    .AddButtonRight("➕📋", $"/book_select_time/{table.Id}")
                    .AddButtonDown("🔲 Столы", $"/tables")
                    .AddButtonRight("В начало", $"/menu"),
                default,
                "AgACAgIAAxkBAAOgZ5pWHbT-EVNXc96-Q0oD7LZCnGMAAjzqMRtkA9FI7XDK_OV9DSQBAAMCAAN4AAM2BA"
                );
        }

        //todo
        [ApiPointer("show_book")]
        private string ShowBook(Book book)
        {
            if (book.IsCanceled)
            {
                return SendOrEdit(
                book.GetState(),
                new InlineKeyboardConstructor()
                    .AddButtonDown("🟢", $"/try_repair/{book.Id}")
                    .AddButtonRight("🔲", $"/table/{book.Table.Id}")
                    .AddButtonDown("В начало", $"/menu")
                );
            }
            else if(book.TableClosed != default)
            {
                return SendOrEdit(
                    book.GetState(),
                    new InlineKeyboardConstructor()
                        .AddButtonDown("↔️", $"/try_prolongate/{book.Id}")
                        .AddButtonRight("🔲", $"/table/{book.Table.Id}")
                        .AddButtonDown("В начало", $"/menu")
                    );
            }
            else if (book.TableStarted != default)
            {
                return SendOrEdit(
                    book.GetState(),
                    new InlineKeyboardConstructor()
                        .AddButtonDown("⛔️", $"/try_close/{book.Id}")
                        .AddButtonRight("↔️", $"/try_prolongate/{book.Id}")
                        .AddButtonRight("🔲", $"/table/{book.Table.Id}")
                        .AddButtonDown("В начало", $"/menu")
                    );
            }
            else
            {
                return SendOrEdit(
                    book.GetState(),
                    new InlineKeyboardConstructor()
                        .AddButtonDown("🔴", $"/try_cancel_book/{book.Id}")
                        .AddButtonRightIf(() => new BookService().CanMove(book), "⤵️", $"/try_move/{book.Id}")
                        .AddButtonRight("✅", $"/try_start_book/{book.Id}")
                        .AddButtonRight("🔲", $"/table/{book.Table.Id}")
                        .AddButtonDown("В начало", $"/menu")
                    );
            }
        }

        [ApiPointer("try_move")]
        private string TryMove(Book book)
        {
            var vars = BookService.GetMoveVariants(book);
            string text = vars.Any()
                ? $"Варианты переноса стола:"
                : $"Этот стол нельзя перенести";
            return SendOrEdit(
                $"{book.GetState()}" +
                $"\r\n\r\n{text}",
                new InlineKeyboardConstructor()
                    .AddHostesMoveVariantsButtons(book, vars)
                    .AddButtonDown("Назад", $"/show_book/{book.Id}"));
        }

        [ApiPointer("move")]
        private string Move(Book book, DateTime newActualTime)
        {
            book.ActualBookStartTime = newActualTime;
            BookDS.Save(book);
            return ShowBook(book);
        }

        [ApiPointer("try_prolongate")]
        private string TryProlongate(Book book)
        {
            var vars = BookService.GetProlongationVariants(book);
            string text = vars.Any()
                ? $"Варианты продления стола:"
                : $"Этот стол нельзя продлить";
            return SendOrEdit(
                $"{book.GetState()}" +
                $"\r\n\r\n{text}",
                new InlineKeyboardConstructor()
                    .AddHostesProlongationVariantsButtons(book, vars)
                    .AddButtonDown("Назад", $"/show_book/{book.Id}"));
        }

        [ApiPointer("pg")]
        private string Prolongate(Book book, DateTime newEndTime)
        {
            book.SetNewBookEndTime(newEndTime);
            BookDS.Save(book);
            return ShowBook(book);
        }

        [ApiPointer("try_start_book")]
        private string TryStartBook(Book book)
        {
            var smena = BookService.GetCurrentSmena();
            var allReadyOpened = BookService.GetCurrentBooks(book.Table)
                .FirstOrDefault(x => x != book && x.GetStatus() == Bronya.Enums.BookStatus.Opened);
            if (allReadyOpened == default)
            {
                string text = $"Открыть стол и поставить отметку о вынесенном кальяне?";
                double diffInMinutes = book.ActualBookStartTime.Subtract(new TimeService().GetNow()).TotalMinutes;
                if (Math.Abs(diffInMinutes) > smena.Schedule.Buffer.TotalMinutes)
                {
                    var trueText = diffInMinutes > 0 ? "рано" : "поздно";
                    text += $"\r\n*Обратите внимание*, гость пришел слишком {trueText}. Проверьте себя.";
                }
                return SendOrEdit(
                    $"{book.GetState()}" +
                    $"\r\n\r\n{text}",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("✅Кальян вынесен✅", $"/start_book/{book.Id}")
                        .AddButtonDown("Назад", $"/show_book/{book.Id}"));
            }
            else
            {
                string text = $"*Предыдущая бронь на имя {allReadyOpened.Account.ToString()} не была закрыта. Проверьте себя.*";
                double diffInMinutes = book.ActualBookStartTime.Subtract(new TimeService().GetNow()).TotalMinutes;
                if (Math.Abs(diffInMinutes) > smena.Schedule.Buffer.TotalMinutes)
                {
                    var trueText = diffInMinutes > 0 ? "рано" : "поздно";
                    text += $"\r\n*Обратите внимание*, гость пришел слишком {trueText}. Проверьте себя.";
                }
                return SendOrEdit(
                    $"{book.GetState()}" +
                    $"\r\n\r\n{text}",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("✅Закрыть предыдущий стол и открыть этот✅", $"/start_book/{book.Id}")
                        .AddButtonDown("Назад", $"/show_book/{book.Id}"));
            }
        }

        [ApiPointer("start_book")]
        private string StartBook(Book book)
        {
            var allReadyOpened = BookService.GetCurrentBooks(book.Table)
                .FirstOrDefault(x => x != book && x.GetStatus() == Bronya.Enums.BookStatus.Opened);
            if (allReadyOpened != default)
            {
                Close(allReadyOpened);
            }
            book.TableStarted = DateTime.Now;
            BookDS.Save(book);
            return ShowBook(book);
        }

        [ApiPointer("try_cancel_book")]
        private string TryCancelBook(Book book)
        {
            if (!new BookService().CanCancel(book))
            {
                return SendOrEdit(
                    $"{book.GetState()}" +
                    $"\r\n\r\n *Бронь не получится отменить*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/show_book/{book.Id}"));
            }
            return SendOrEdit(
                $"{book.GetState()}" +
                $"\r\n*Отменить бронь на {book.ActualBookStartTime:dd.MM HH:mm}?*",
                new InlineKeyboardConstructor()
                    .AddButtonDown("🔴Отменить🔴", $"/cancel_book/{book.Id}")
                    .AddButtonDown("Назад", $"/show_book/{book.Id}"));
        }

        [ApiPointer("cancel_book")]
        private string CancelBook(Book book)
        {
            if (!new BookService().Cancel(book))
            {
                return SendOrEdit(
                    $"*Бронь не получилось отменить*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/show_book/{book.Id}"));
            }
            book.IsCanceled = true;
            BookDS.Save(book);
            return ShowBook(book);
        }
        
        [ApiPointer("try_repair")]
        private string TryRepair(Book book)
        {
            if (!new BookService().CanRepair(book))
            {
                return SendOrEdit(
                    $"{book.GetState()}" +
                    $"\r\n\r\n *Восстановить бронь не получится*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/show_book/{book.Id}"));
            }
            return SendOrEdit(
                $"{book.GetState()}" +
                $"\r\n*Восстановить бронь на {book.ActualBookStartTime:dd.MM HH:mm}?*",
                new InlineKeyboardConstructor()
                    .AddButtonDown("🟢Восстановить🟢", $"/repair/{book.Id}")
                    .AddButtonDown("Назад", $"/show_book/{book.Id}"));
        }

        [ApiPointer("repair")]
        private string Repair(Book book)
        {
            if (!new BookService().CanRepair(book))
            {
                return SendOrEdit(
                    $"*Бронь не получилось восстановить*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/show_book/{book.Id}"));
            }
            book.IsCanceled = false;
            BookDS.Save(book);
            return ShowBook(book);
        }
        
        [ApiPointer("try_close")]
        private string TryClose(Book book)
        {
            if (book.TableStarted == default)
            {
                return SendOrEdit(
                    $"{book.GetState()}" +
                    $"\r\n\r\n *Закрыть стол не получится*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/show_book/{book.Id}"));
            }
            return SendOrEdit(
                $"{book.GetState()}" +
                $"\r\n*Закрыть стол?*",
                new InlineKeyboardConstructor()
                    .AddButtonDown("⛔️Закрыть⛔️", $"/close/{book.Id}")
                    .AddButtonDown("Назад", $"/show_book/{book.Id}"));
        }

        [ApiPointer("close")]
        private string Close(Book book)
        {
            book.TableClosed = DateTime.Now;
            BookDS.Save(book);
            return ShowBook(book);
        }

        [ApiPointer("disable")]
        private string Disable(Table table)
        {
            table.IsBookAvailable = false;
            TableDS.Save(table);
            return Table(table);
        }

        [ApiPointer("enable")]
        private string Enable(Table table)
        {
            table.IsBookAvailable = true;
            TableDS.Save(table);
            return Table(table);
        }

        [ApiPointer("book_select_time")]
        private string BookSelectTime(Table table = default)
        {
            if (table != null)
            {
                Package.Account.SelectedTable = table;
                AccountService.AccountDS.Save(Package.Account);
            }
            if (Package.Account.SelectedTime != default)
            {
                Package.Account.SelectedTime = default;
                AccountService.AccountDS.Save(Package.Account);
            }

            table = Package.Account.SelectedTable;

            var times = table != null
                ? BookService.GetAvailableTimesForBook(table)
                : BookService.GetAvailableTimesForBook();

            var backCallback = table != null
                ? $"/table/{table.Id}"
                : $"/reset_all";

            return SendOrEdit(
                $"{Package.Account.GetNewBookState()}" +
                $"\r\n*Выбор времени:*",
                new InlineKeyboardConstructor()
                    .AddTimeButtons(times)
                    .AddButtonDown("🗑", backCallback)
                    .AddButtonRightIf(() => table != null, "♻️⏱️", "/reset_table")
                    .AddButtonRight(table != null ? "✏️🔲" : "✏️🔲", "/select_table")
                );
        }

        /// <summary>
        /// Сбросить время и выбрать стол
        /// </summary>
        /// <param name="Package.Account"></param>
        /// <param name="time"></param>
        /// <param name=""></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [ApiPointer("reset_time")]
        private string ResetTime()
        {
            Package.Account.SelectedTime = default;
            AccountService.AccountDS.Save(Package.Account);
            return SelectTable();
        }

        /// <summary>
        /// Сбросить стол и выбрать время
        /// </summary>
        /// <param name="Package.Account"></param>
        /// <param name="time"></param>
        /// <param name=""></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [ApiPointer("reset_table")]
        private string ResetTable()
        {
            Package.Account.SelectedTable = default;
            AccountService.AccountDS.Save(Package.Account);
            return BookSelectTime();
        }

        /// <summary>
        /// Сбросить стол, время, места и имя гостя
        /// </summary>
        /// <param name="Package.Account"></param>
        /// <param name="time"></param>
        /// <param name=""></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [ApiPointer("reset_all")]
        private string ResetAll()
        {
            Package.Account.SelectedTable = default;
            Package.Account.SelectedTime = default;
            Package.Account.SelectedPlaces = default;
            Package.Account.Waiting = default;
            AccountService.AccountDS.Save(Package.Account);
            return Menu();
        }

        [ApiPointer("set_time")]
        private string SetTime(DateTime time)
        {
            Package.Account.SelectedTime = time;
            AccountService.AccountDS.Save(Package.Account);
            if (Package.Account.SelectedTable == null)//стол еще не выбран
            {
                return SelectTable();
            }
            else
            {
                return SelectPlaces();
            }
        }

        [ApiPointer("select_table")]
        private string SelectTable()
        {
            var tables = Package.Account.SelectedTime != default
                ? TableDS.GetAll().Where(x => x.IsBookAvailable).OrderBy(x => x.Number)
                    .ToArray()
                    .Where(table =>
                    {
                        var times = BookService.GetAvailableTimesForBook(table);
                        return times.Contains(Package.Account.SelectedTime);//Поиск по конкретному времени
                    }).ToArray()
                : TableDS.GetAll().Where(x => x.IsBookAvailable).OrderBy(x => x.Number)
                    .ToArray()
                    .Where(table =>
                    {
                        var times = BookService.GetAvailableTimesForBook(table);//Поиск столов, у которых есть свободное время
                        return times.Any();
                    }).ToArray();

            return SendOrEdit(
                    $"{Package.Account.GetNewBookState()}" +
                    $"\r\n*Выбор стола:*",
                    new InlineKeyboardConstructor()
                        .AddHostesTableButtons(tables)
                        .AddButtonDown("🗑", $"/reset_all")
                        .AddButtonRight(Package.Account.SelectedTime != default ? "✏️⏱️" : "✏️⏱️", "/book_select_time")
                        .AddButtonRightIf(() => Package.Account.SelectedTime != default, "♻️🔲", "/reset_time"),
                    default,
                    "AgACAgIAAxkBAAOgZ5pWHbT-EVNXc96-Q0oD7LZCnGMAAjzqMRtkA9FI7XDK_OV9DSQBAAMCAAN4AAM2BA"
                    );
        }

        [ApiPointer("set_table")]
        private string SetTable(Table table)
        {
            Package.Account.SelectedTable = table;
            AccountService.AccountDS.Save(Package.Account);

            if (Package.Account.SelectedTime != default)
            {
                return SelectPlaces();
            }
            else
            {
                return BookSelectTime();
            }
        }

        [ApiPointer("select_places")]
        private string SelectPlaces()
        {
            return SendOrEdit(
                $"{Package.Account.GetNewBookState()}" +
                $"\r\n*Гостей:*",
                new InlineKeyboardConstructor()
                    .AddHostesPlacesButtons(Package.Account.SelectedTable)
                    .AddButtonDown("🗑", $"/reset_all")
                    .AddButtonRight("✏️⏱️", $"/book_select_time")
                    .AddButtonRight("✏️🔲", $"/select_table")
                );
        }

        [ApiPointer("set_places")]
        private string SetPlaces(int places)
        {
            Package.Account.SelectedPlaces = places;
            AccountService.AccountDS.Save(Package.Account);

            return SelectName();
        }

        [ApiPointer("select_name")]
        private string SelectName()
        {
            Package.Account.Waiting = WaitingText.Name;
            AccountService.AccountDS.Save(Package.Account);

            return SendOrEdit(
                $"{Package.Account.GetNewBookState()}" +
                $"\r\n*Имя брони:*",
                new InlineKeyboardConstructor()
                    .AddButtonDown("🗑", $"/reset_all")
                    .AddButtonRight("✏️⏱️", $"/book_select_time")
                    .AddButtonRight("✏️🔲", $"/select_table")
                    .AddButtonRight("✏️👤", $"/select_places")
                );
        }

        [ApiPointer("set_name")]
        private string SetName(string name)
        {
            var smena = BookService.GetCurrentSmena();
            var newBook = new Book()
            {
                SeatAmount = Package.Account.SelectedPlaces,
                ActualBookStartTime = Package.Account.SelectedTime,
                BookLength = smena.Schedule.MinPeriod,
                Table = Package.Account.SelectedTable,
                Account = new Account()
                {
                    Name = name,
                }
            };
            BookDS.CascadeSave(newBook);

            Package.Account.Waiting = default;
            Package.Account.SelectedPlaces = default;
            Package.Account.SelectedTable = default;
            Package.Account.SelectedTime = default;
            AccountService.AccountDS.Save(Package.Account);

            return ShowBook(newBook);
        }
    }
}
