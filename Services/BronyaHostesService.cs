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
using static System.Net.Mime.MediaTypeNames;

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

        [TGPointer("start", "menu")]
        private string Menu()
        {
            return SendOrEdit(
                "Меню хостеса:",
                new InlineKeyboardConstructor()
                    .AddButtonDown("🔲 Столы", "/tables")
                    .AddButtonDown("➕📋 Новая бронь", "/book_select_time")
                );
        }

        [TGPointer("tables")]
        private string Tables()
        {
            return SendOrEdit(
                "Меню хостеса:",
                new InlineKeyboardConstructor()
                    .AddAllHostesTableButtons()
                    .AddButtonDown("Назад", $"/menu")
                );
        }

        [TGPointer("table")]
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
                    .AddButtonDown("➕📋", $"/book_select_time/{table.Id}")
                    .AddButtonDownIf(() => table.IsBookAvailable, "🚫 Отключить бронирование", $"/disable/{table.Id}")
                    .AddButtonDownIf(() => !table.IsBookAvailable, "✅ Включить бронирование", $"/enable/{table.Id}")
                    .AddButtonDown("В начало", $"/menu")
                    .AddButtonRight("🔲 Столы", $"/tables")
                );
        }

        //todo
        [TGPointer("show_book")]
        private string ShowBook(Book book)
        {
            if (book.IsCanceled)
            {
                return SendOrEdit(
                book.GetState(),
                new InlineKeyboardConstructor()
                    .AddButtonDown("🟢", $"/try_repair/{book.Id}")
                    .AddButtonRight("🔲", $"/table/{book.Table.Id}")
                );
            }
            else if(book.TableClosed != default)
            {
                return SendOrEdit(
                    book.GetState(),
                    new InlineKeyboardConstructor()
                        .AddButtonDown("↔️", $"/try_prolongate/{book.Id}")//todo
                        .AddButtonRight("🔲", $"/table/{book.Table.Id}")
                    );
            }
            else if (book.TableStarted != default)
            {
                return SendOrEdit(
                    book.GetState(),
                    new InlineKeyboardConstructor()
                        .AddButtonDown("⛔️", $"/try_close/{book.Id}")
                        .AddButtonRight("↔️", $"/try_prolongate/{book.Id}")//todo
                        .AddButtonRight("🔲", $"/table/{book.Table.Id}")
                    );
            }
            else
            {
                return SendOrEdit(
                    book.GetState(),
                    new InlineKeyboardConstructor()
                        .AddButtonDown("🔴", $"/try_cancel_book/{book.Id}")
                        .AddButtonRightIf(() => new BookService().CanMove(book), "⤵️", $"/move_times/{book.Id}")
                        .AddButtonRight("✅", $"/try_start_book/{book.Id}")
                        .AddButtonRight("🔲", $"/table/{book.Table.Id}")
                    );
            }
        }

        [TGPointer("try_start_book")]
        private string TryStartBook(Book book)
        {
            var smena = BookService.GetCurrentSmena();
            string text = $"Открыть стол и поставить отметку о вынесенном кальяне?";
            double diffInMinutes = book.ActualBookStartTime.Subtract(DateTime.Now).TotalMinutes;
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

        [TGPointer("start_book")]
        private string StartBook(Book book)
        {
            book.TableStarted = DateTime.Now;
            BookDS.Save(book);
            return ShowBook(book);
        }

        [TGPointer("try_cancel_book")]
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

        [TGPointer("cancel_book")]
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
        
        [TGPointer("try_repair")]
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

        [TGPointer("repair")]
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
        
        [TGPointer("try_close")]
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

        [TGPointer("close")]
        private string Close(Book book)
        {
            book.TableClosed = DateTime.Now;
            BookDS.Save(book);
            return ShowBook(book);
        }

        [TGPointer("disable")]
        private string Disable(Table table)
        {
            table.IsBookAvailable = false;
            TableDS.Save(table);
            return Table(table);
        }

        [TGPointer("enable")]
        private string Enable(Table table)
        {
            table.IsBookAvailable = true;
            TableDS.Save(table);
            return Table(table);
        }

        [TGPointer("book_select_time")]
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
        [TGPointer("reset_time")]
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
        [TGPointer("reset_table")]
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
        [TGPointer("reset_all")]
        private string ResetAll()
        {
            Package.Account.SelectedTable = default;
            Package.Account.SelectedTime = default;
            Package.Account.SelectedPlaces = default;
            Package.Account.Waiting = default;
            AccountService.AccountDS.Save(Package.Account);
            return Menu();
        }

        [TGPointer("set_time")]
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

        [TGPointer("select_table")]
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
                        .AddButtonRightIf(() => Package.Account.SelectedTime != default, "♻️🔲", "/reset_time")
                    );
        }

        [TGPointer("set_table")]
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

        [TGPointer("select_places")]
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

        [TGPointer("set_places")]
        private string SetPlaces(int places)
        {
            Package.Account.SelectedPlaces = places;
            AccountService.AccountDS.Save(Package.Account);

            return SelectName();
        }

        [TGPointer("select_name")]
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

        [TGPointer("set_name")]
        private string SetName(string name)
        {
            var newBook = new Book()
            {
                SeatAmount = Package.Account.SelectedPlaces,
                ActualBookStartTime = Package.Account.SelectedTime,
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
