using Buratino.Xtensions;
using Bronya.Entities;
using Bronya.Enums;
using Bronya.Xtensions;
using Bronya.API;
using Bronya.Helpers;
using Bronya.Attributes;
using System.Data;

namespace Bronya.Services
{
    /// <summary>
    /// Сервис взаимодействия с пользователем через telegram
    /// </summary>
    public class BronyaHostesService : BronyaServiceBase
    {
        public BronyaHostesService(LogToFileService logService, TGAPI tGAPI, Account account) : base(logService, tGAPI, account)
        {
        }

        [ApiPointer("start", "menu")]
        private string Menu()
        {
            //Сброс на всякий случай
            if (Package.Account.SelectedBook != default)
            {
                Package.Account.SelectedTable = default;
                Package.Account.SelectedTime = default;
                AccountService.SelectBook(Package.Account, default);
            }

            return SendOrEdit(
                "Меню хостеса:",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Сейчас", "/now")
                    .AddButtonDown("🔲 Столы", "/tables")
                    .AddButtonDown("➕📋 Новая бронь", "/book_select_time")
                    .AddButtonDown("👤Гости", "/get_accounts")
                    .AddButtonDownIf(SafeCheck, "Роль", "/show_role")
                );
        }

        [ApiPointer("tables")]
        private string Tables()
        {
            var stream = new CalendarDrawService().DrawFull();
            return SendOrEdit(
                "Столы:",
                new InlineKeyboardConstructor()
                    .AddHostesAllTableButtons(Package.Account)
                    .AddButtonDown("Назад", $"/menu"),
                default,
                stream
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
            var stream = new CalendarDrawService().Draw(table);
            var books = BookService.GetCurrentBooks(table, true);
            var isAvailable = !table.IsBookAvailable ? "\r\n🚫 Бронь отключена" : string.Empty;
            return SendOrEdit(
                $"Стол: {table.Name.EscapeFormat()}{isAvailable}",
                new InlineKeyboardConstructor()
                    .AddHostesBooksButtons(books, "/show_book")
                    .AddButtonDownIf(() => table.IsBookAvailable, "🚫 Отключить бронирование", $"/disable/{table.Id}")
                    .AddButtonDownIf(() => !table.IsBookAvailable, "✅ Включить бронирование", $"/enable/{table.Id}")
                    .AddButtonRight("➕📋", $"/book_select_time/{table.Id}")
                    .AddButtonDown("🔲 Столы", $"/tables"),
                default,
                stream
            );
        }

        [ApiPointer("show_book")]
        private string ShowBook(Book book)
        {
            return SendOrEdit(
                book.GetState(),
                book.GetButtons()
                    .AddButtonDown("В начало", $"/menu")
            );
        }

        [ApiPointer("now")]
        public string Now()
        {
            var stream = new CalendarDrawService().DrawFull();
            var msgId = SendOrEdit(
                "Сейчас:",
                new InlineKeyboardConstructor()
                    .AddHostesNowTableButtons(Package.Account)
                    .AddButtonDown("Назад", $"/menu"),
                default,
                stream
            );
            Package.Account.NowMenuMessageId = msgId.AsInt();
            AccountService.AccountDS.Save(Package.Account);
            return msgId;
        }

        [ApiPointer("now_table")]
        private string NowTable(Table table)
        {
            if (Package.Account.SelectedTable != default)
            {
                Package.Account.SelectedTable = default;
                AccountService.AccountDS.Save(Package.Account);
            }
            var stream = new CalendarDrawService().Draw(table);
            var books = BookService.GetCurrentBooks(table, true);
            var isAvailable = !table.IsBookAvailable ? "\r\n🚫 Бронь отключена" : string.Empty;
            return SendOrEdit(
                $"Стол: {table.Name.EscapeFormat()}{isAvailable}",
                new InlineKeyboardConstructor()
                    .AddHostesBooksButtons(books, "/show_book")
                    .AddButtonDownIf(() => table.IsBookAvailable, "🚫 Отключить бронирование", $"/disable/{table.Id}")
                    .AddButtonDownIf(() => !table.IsBookAvailable, "✅ Включить бронирование", $"/enable/{table.Id}")
                    .AddButtonRight("➕📋", $"/book_select_time/{table.Id}")
                    .AddButtonDown("Сейчас", $"/now"),
                default,
                stream
            );
        }

        [ApiPointer("now_show_book")]
        private string NowShowBook(Book book)
        {
            return SendOrEdit(
                book.GetState(),
                book.GetButtons()
                    .AddButtonDown("Сейчас", $"/now")
            );
        }

        #region book manage
        [ApiPointer("try_move")]
        private string TryMove(Book book)
        {
            var vars = BookService.GetMoveVariants(book, Package.Account);
            string text = vars.Length != 0
                ? $"Варианты переноса стола:"
                : $"\r\nЭтот стол нельзя перенести";
            return SendOrEdit(
                $"{book.GetState()}" +
                $"\r\n\r\n{text}",
                new InlineKeyboardConstructor()
                    .AddHostesMoveVariantsButtons(book, vars)
                    .AddButtonDown("Назад", $"/show_book/{book.Id}")
            );
        }

        [ApiPointer("move")]
        private string Move(Book book, DateTime newActualTime)
        {
            LogService.LogEvent(nameof(Move) + ":" + book?.Id + ":" + newActualTime.ToString());
            book.ActualBookStartTime = newActualTime;
            BookService.BookDS.Save(book);
            return ShowBook(book);
        }

        [ApiPointer("try_prolongate")]
        private string TryProlongate(Book book)
        {
            var vars = BookService.GetProlongationVariants(book, Package.Account);
            string text = vars.Length != 0
                ? $"Варианты продления стола:"
                : $"\r\nЭтот стол нельзя продлить";
            return SendOrEdit(
                $"{book.GetState()}" +
                $"\r\n\r\n{text}",
                new InlineKeyboardConstructor()
                    .AddHostesProlongationVariantsButtons(book, vars)
                    .AddButtonDown("Назад", $"/show_book/{book.Id}")
            );
        }

        [ApiPointer("pg")]
        private string Prolongate(Book book, DateTime newEndTime)
        {
            LogService.LogEvent(nameof(Prolongate) + ":" + book?.Id + ":" + newEndTime.ToString());
            book.SetNewBookEndTime(newEndTime);
            book.NotifiedAboutEndBook = default;
            book.TableClosed = default;
            BookService.BookDS.Save(book);
            return ShowBook(book);
        }

        [ApiPointer("try_start_book")]
        private string TryStartBook(Book book)
        {
            var smena = BookService.Smena;
            var allReadyOpened = BookService
                .GetCurrentBooks(book.Table)
                .FirstOrDefault(x => x != book && x.GetStatus() == BookStatus.Opened);
            if (allReadyOpened == default)
            {
                string text = $"Открыть стол и поставить отметку о вынесенном кальяне?";
                double diffInMinutes = book.ActualBookStartTime.Subtract(new TimeService().GetNow()).TotalMinutes;
                if (Math.Abs(diffInMinutes) > smena.Schedule.Buffer.TotalMinutes)
                {
                    var trueText = diffInMinutes > 0 ? "рано" : "поздно";
                    text += $"\r\n*Обратите внимание*, гость пришел слишком {trueText}\\. Проверьте себя\\.";
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
                string text = $"*Предыдущая бронь на имя {allReadyOpened.Guest} не была закрыта\\. Проверьте себя\\.*";
                double diffInMinutes = book.ActualBookStartTime.Subtract(new TimeService().GetNow()).TotalMinutes;
                if (Math.Abs(diffInMinutes) > smena.Schedule.Buffer.TotalMinutes)
                {
                    var trueText = diffInMinutes > 0 ? "рано" : "поздно";
                    text += $"\r\n*Обратите внимание*, гость пришел слишком {trueText}\\. Проверьте себя\\.";
                }
                return SendOrEdit(
                    $"{book.GetState()}" +
                    $"\r\n\r\n{text}",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("✅Закрыть предыдущую бронь и вынести кальян✅", $"/start_book/{book.Id}")
                        .AddButtonDown("Назад", $"/show_book/{book.Id}"));
            }
        }

        [ApiPointer("start_book")]
        private string StartBook(Book book)
        {
            LogService.LogEvent(nameof(StartBook) + ":" + book?.Id);
            var smena = BookService.Smena;
            var allReadyOpened = BookService.GetCurrentBooks(book.Table)
                .FirstOrDefault(x => x != book && x.GetStatus() == BookStatus.Opened);
            if (allReadyOpened != default)
            {
                Close(allReadyOpened);
            }
            book.TableStarted = new TimeService().GetNow();
            book.TableAllowedStarted = book.ActualBookStartTime.Add(smena.Schedule.Buffer) < book.TableStarted
                ? book.ActualBookStartTime.Add(smena.Schedule.Buffer)
                : book.TableStarted;
            BookService.BookDS.Save(book);
            return ShowBook(book);
        }

        [ApiPointer("try_cancel")]
        private string TryCancel(Book book)
        {
            if (!BookService.CanCancel(book))
            {
                return SendOrEdit(
                    $"{book.GetState()}" +
                    $"\r\n\r\n *Бронь не получится отменить*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/show_book/{book.Id}"));
            }
            return SendOrEdit(
                $"{book.GetState()}" +
                $"\r\n*Отменить бронь на {book.ActualBookStartTime.ToddMM_HHmm()}?*",
                new InlineKeyboardConstructor()
                    .AddButtonDown("🔴Отменить🔴", $"/cancel/{book.Id}")
                    .AddButtonDown("Назад", $"/show_book/{book.Id}"));
        }

        [ApiPointer("cancel")]
        private string Cancel(Book book)
        {
            LogService.LogEvent(nameof(Cancel) + ":" + book?.Id);
            if (!BookService.Cancel(book))
            {
                return SendOrEdit(
                    $"*Бронь не получилось отменить*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/show_book/{book.Id}")
                );
            }
            book.IsCanceled = true;
            BookService.BookDS.Save(book);
            return ShowBook(book);
        }

        [ApiPointer("try_repair")]
        private string TryRepair(Book book)
        {
            if (!BookService.CanRepair(book, Package.Account))
            {
                return SendOrEdit(
                    $"{book.GetState()}" +
                    $"\r\n\r\n *Восстановить бронь не получится*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/show_book/{book.Id}")
                );
            }
            return SendOrEdit(
                $"{book.GetState()}" +
                $"\r\n*Восстановить бронь на {book.ActualBookStartTime.ToddMM_HHmm()}?*",
                new InlineKeyboardConstructor()
                    .AddButtonDown("🟢Восстановить🟢", $"/repair/{book.Id}")
                    .AddButtonDown("Назад", $"/show_book/{book.Id}")
            );
        }

        [ApiPointer("repair")]
        private string Repair(Book book)
        {
            LogService.LogEvent(nameof(Repair) + ":" + book?.Id);
            if (!BookService.CanRepair(book, Package.Account))
            {
                return SendOrEdit(
                    $"*Бронь не получилось восстановить*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/show_book/{book.Id}")
                );
            }
            book.IsCanceled = false;
            BookService.BookDS.Save(book);
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
            LogService.LogEvent(nameof(Close) + ":" + book?.Id);
            book.TableClosed = new TimeService().GetNow();
            BookService.BookDS.Save(book);
            return ShowBook(book);
        }

        [ApiPointer("edit")]
        private string Edit(Book book)
        {
            LogService.LogEvent(nameof(Edit) + ":" + book?.Id);

            if (!book.GetStatus().In(BookStatus.Booked, BookStatus.Opened))
            {
                return ShowBook(book);
            }
            var minTime = BookService.Smena.GetMinimumTimeToBook(null);
            Package.Account.SelectedTable = default;
            Package.Account.SelectedTime = book.ActualBookStartTime > minTime ? book.ActualBookStartTime : minTime;
            AccountService.SelectBook(Package.Account, book);

            return SelectTable();
        }

        private string EditPreview()
        {
            LogService.LogEvent(nameof(EditPreview) + ":" + Package.Account.SelectedBook?.Id);

            return SendOrEdit(
                Package.Account.SelectedBook.GetEditState(Package.Account),
                new InlineKeyboardConstructor()
                    .AddButtonDown("Отмена", $"/reset_all")
                    .AddButtonRight("✏️⏱️", $"/book_select_time")
                    .AddButtonRight("✏️🔲", $"/select_table")
                    .AddButtonDown("Сохранить", $"/update_book")
            );
        }

        [ApiPointer("update_book")]
        private string UpdateBook()
        {
            var book = Package.Account.SelectedBook;
            var availables = BookService.GetAvailableTimesForBook(Package.Account.SelectedTable, Package.Account, book);
            if (!availables.Contains(Package.Account.SelectedTime))
            {
                return SendOrEdit(
                    "Теперь нельзя забронировать стол на это время",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Поменять время", $"/reset_time")
                );
            }

            if (book.GetStatus() == BookStatus.Opened)
            {
                var newBook = new Book()
                {
                    Guest = book.Guest,
                    SeatAmount = book.SeatAmount,
                    ActualBookStartTime = Package.Account.SelectedTime,
                    BookLength = book.BookLength - (Package.Account.SelectedTime - book.TableAllowedStarted),
                    TableStarted = Package.Account.SelectedTime,
                    TableAllowedStarted = Package.Account.SelectedTime,
                    Table = Package.Account.SelectedTable,
                };
                BookService.BookDS.Save(newBook);

                book.SetNewBookEndTime(Package.Account.SelectedTime);
                book.TableClosed = Package.Account.SelectedTime;
                BookService.BookDS.Save(book);

                Package.Account.SelectedTable = default;
                Package.Account.SelectedTime = default;
                AccountService.SelectBook(Package.Account, default);
                return ShowBook(newBook);
            }
            else if (book.GetStatus() == BookStatus.Booked)
            {
                book.ActualBookStartTime = Package.Account.SelectedTime;
                book.Table = Package.Account.SelectedTable;
                BookService.BookDS.Save(book);

                Package.Account.SelectedTable = default;
                Package.Account.SelectedTime = default;
                AccountService.SelectBook(Package.Account, default);
                return ShowBook(book);
            }
            else
            {
                return ShowBook(book);
            }
        }
        #endregion

        #region table manage
        [ApiPointer("disable")]
        private string Disable(Table table)
        {
            LogService.LogEvent(nameof(Disable) + ":" + table?.Name);
            table.IsBookAvailable = false;
            BookService.TableDS.Save(table);
            return Table(table);
        }

        [ApiPointer("enable")]
        private string Enable(Table table)
        {
            LogService.LogEvent(nameof(Enable) + ":" + table?.Name);
            table.IsBookAvailable = true;
            BookService.TableDS.Save(table);
            return Table(table);
        }
        #endregion

        #region book creation
        [ApiPointer("book_select_time")]
        private string BookSelectTime(Table table = default)
        {
            LogService.LogEvent(nameof(BookSelectTime) + ":" + table?.Name);
            if (table != default)
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

            var times = table != default
                ? BookService.GetAvailableTimesForBook(table, Package.Account)
                : BookService.GetAvailableTimesForBook(Package.Account);

            var backCallback = table != default
                ? $"/table/{table.Id}"
                : $"/reset_all";

            var paramText = Package.Account.SelectedBook == default
                ? Package.Account.GetNewBookState()
                : Package.Account.SelectedBook.GetEditState(Package.Account);

            var footerText = table != default
                ? $"Выбор времени на стол {table}:"
                : $"Выбор времени:";

            return SendOrEdit(
                $"{paramText}" +
                $"\r\n\r\n*{footerText}*",
                new InlineKeyboardConstructor()
                    .AddTimeButtons(times)
                    .AddButtonDown("🗑", backCallback)
                    .AddButtonRightIf(() => table != null, "♻️⏱️", "/reset_table")
                    .AddButtonRight(table != null ? "✏️🔲" : "✏️🔲", "/select_table"),
                null,
                new CalendarDrawService().DrawFull()
            );
        }

        [ApiPointer("set_time")]
        private string SetTime(DateTime time)
        {
            LogService.LogEvent(nameof(SetTime) + ":" + time.ToString());
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
            var allBooks = BookService.GetCurrentBooks();
            Table[] tables;
            if (Package.Account.SelectedTime != default)
            {
                tables = BookService.TableDS.GetAll().Where(x => x.IsBookAvailable).OrderBy(x => x.Number)
                    .ToArray()
                    .Where(table =>
                    {
                        //Поиск по конкретному времени
                        return BookService.GetAvailableTimesForBook(
                            table,
                            Package.Account,
                            Package.Account.SelectedBook,
                            allBooks.Where(x => x.Table == table).ToList())
                        .Contains(Package.Account.SelectedTime);
                    }).ToArray();
            }
            else
            {
                tables = BookService.TableDS.GetAll().Where(x => x.IsBookAvailable).OrderBy(x => x.Number)
                    .ToArray()
                    .Where(table =>
                    {
                        //Поиск столов, у которых есть свободное время
                        return BookService.GetAvailableTimesForBook(
                            table,
                            Package.Account,
                            Package.Account.SelectedBook,
                            allBooks.Where(x => x.Table == table).ToList()).Length != 0;
                    }).ToArray();
            }

            var paramText = Package.Account.SelectedBook == default
                ? Package.Account.GetNewBookState()
                : Package.Account.SelectedBook.GetEditState(Package.Account);

            var footerText = Package.Account.SelectedTime != default
                ? $"Выбор стола на {Package.Account.SelectedTime.ToHHmm()}:"
                : $"Выбор стола:";

            return SendOrEdit(
                $"{paramText}" +
                $"\r\n\r\n*{footerText}*",
                new InlineKeyboardConstructor()
                    .AddHostesTableButtons(tables, Package.Account)
                    .AddButtonDown("🗑", $"/reset_all")
                    .AddButtonRight(Package.Account.SelectedTime != default ? "✏️⏱️" : "✏️⏱️", "/book_select_time")
                    .AddButtonRightIf(() => Package.Account.SelectedTime != default, "♻️🔲", "/reset_time"),
                default,
                new CalendarDrawService().DrawFull()
            );
        }

        [ApiPointer("set_table")]
        private string SetTable(Table table)
        {
            LogService.LogEvent(nameof(SetTable) + ":" + table?.Name);
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
            //Редактирование брони
            if (Package.Account.SelectedBook != default)
            {
                return EditPreview();
            }
            return SendOrEdit(
                $"{Package.Account.GetNewBookState()}" +
                $"\r\n\r\n*Гостей:*",
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
            LogService.LogEvent(nameof(SetPlaces) + ":" + places);
            Package.Account.SelectedPlaces = places;
            AccountService.AccountDS.Save(Package.Account);

            return SelectName();
        }

        [ApiPointer("select_name")]
        private string SelectName()
        {
            Package.Account.Waiting = WaitingText.Name;
            AccountService.AccountDS.Save(Package.Account);
            var accs = new RelevantGuestListService().GetAccountsList();

            return SendOrEdit(
                $"{Package.Account.GetNewBookState()}" +
                $"\r\n\r\n*Введите имя брони:*" +
                $"\r\n_Пример: Иван, 1111_",
                new InlineKeyboardConstructor()
                    .AddHostesShowAccounts(accs.Take(6), "set_name_true")
                    .AddButtonDown("🗑", $"/reset_all")
                    .AddButtonRight("✏️⏱️", $"/book_select_time")
                    .AddButtonRight("✏️🔲", $"/select_table")
                    .AddButtonRight("✏️👤", $"/select_places")
                );
        }

        [ApiPointer("set_name")]
        private string SetName(string name)
        {
            LogService.LogEvent(nameof(SetName) + ":" + name);
            var smena = BookService.Smena;
            DateTime minimunBookTime = smena.GetMinimumTimeToBook(Package.Account);
            var isInWindow = Package.Account.SelectedTime.Between_LTE_GTE(minimunBookTime, minimunBookTime.Add(smena.Schedule.Step).Add(smena.Schedule.Step));
            var accs = AccountService.FindAccount(name, isInWindow);
            if (accs == default)
            {
                return SelectName();
            }

            if (accs.Count() == 1)
            {
                return SetNameTrue(accs.First());
            }
            else
            {
                return SendOrEdit(
                    $"{Package.Account.GetNewBookState()}" +
                    $"\r\n\r\n*Уточните имя:*",
                    new InlineKeyboardConstructor()
                        .AddHostesShowAccounts(accs, "set_name_true")
                        .AddButtonDown("✏️👤", $"/select_name")
                        .AddButtonDown("🗑", $"/reset_all")
                        .AddButtonRight("✏️⏱️", $"/book_select_time")
                        .AddButtonRight("✏️🔲", $"/select_table")
                        .AddButtonRight("✏️👤", $"/select_places")
                    );
            }
        }

        [ApiPointer("set_name_true")]
        private string SetNameTrue(Account account)
        {
            LogService.LogEvent(nameof(SetNameTrue) + ":" + account?.Id);
            var smena = BookService.Smena;
            var newBook = new Book()
            {
                SeatAmount = Package.Account.SelectedPlaces,
                ActualBookStartTime = Package.Account.SelectedTime,
                BookLength = smena.Schedule.MinPeriod,
                Table = Package.Account.SelectedTable,
                Guest = account
            };

            BookService.BookDS.Save(newBook);

            Package.Account.Waiting = default;
            Package.Account.SelectedPlaces = default;
            Package.Account.SelectedTable = default;
            Package.Account.SelectedTime = default;
            AccountService.AccountDS.Save(Package.Account);

            return ShowBook(newBook);
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
            LogService.LogEvent(nameof(ResetTime));
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
            LogService.LogEvent(nameof(ResetTable));
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
            LogService.LogEvent(nameof(ResetAll));
            Package.Account.SelectedTable = default;
            Package.Account.SelectedTime = default;
            Package.Account.SelectedPlaces = default;
            Package.Account.Waiting = default;

            var book = Package.Account.SelectedBook;
            AccountService.SelectBook(Package.Account, default);
            if (book != null)
            {
                return ShowBook(book);
            }
            else
            {
                return Menu();
            }
        }
        #endregion

        #region guest management
        [ApiPointer("get_accounts")]
        private string GetAccounts(int page = 1)
        {
            var accs = AccountService.AccountDS.GetAll()
                .Where(x => x.CardNumber != default || x.Phone != default)
                .OrderByDescending(x => x.TimeStamp)
                .ToArray();

            int onPage = 10;
            int total = accs.Length;
            int maxPage = (int)Math.Ceiling(total * 1.0 / onPage);

            if (page < 1)
                page = 1;

            if (page > maxPage)
                page = maxPage;

            if (page != Package.Account.GetAccountsPage)
            {
                Package.Account.GetAccountsPage = page;
                AccountService.AccountDS.Save(Package.Account);
            }
            return SendOrEdit(
                $"Гостей {total}" +
                $"\r\nСтр {page}/{maxPage}",
                new InlineKeyboardConstructor()
                    .AddHostesShowAccounts(accs.Skip((page - 1) * onPage).Take(onPage), "account")
                    .AddButtonDownIf(() => page > 1, "<", $"/get_accounts/{page - 1}")
                    .AddButtonRightIf(() => page < maxPage && page > 1, ">", $"/get_accounts/{page + 1}")
                    .AddButtonDownIf(() => page < maxPage && page == 1, ">", $"/get_accounts/{page + 1}")
                    .AddButtonDown("Назад", $"/menu")
            );
        }

        [ApiPointer("account")]
        private string Account(Account mainAccount)
        {
            //Сброс на всякий случай
            if (Package.Account.Waiting != default)
            {
                Package.Account.SelectedAccount = default;
                Package.Account.Waiting = WaitingText.None;
                AccountService.AccountDS.Save(Package.Account);
            }

            var accs = AccountService.GetAccountsToUnion(mainAccount);
            var trueAcc = AccountService.GetTrueAccount(mainAccount);
            var books = BookService.GetBooks(mainAccount);

            return SendOrEdit(
                $"{mainAccount.GetCard()}",
                new InlineKeyboardConstructor()
                    .AddButtonDownIf(() => books.Any(), "📋 Все брони", $"/account_books/{mainAccount.Id}")
                    .AddButtonDown("✏️ Телефон", $"/select_phone/{mainAccount.Id}")
                    .AddButtonDown("✏️ Карта", $"/select_card/{mainAccount.Id}")
                    .AddButtonDown("✏️ Имя", $"/select_acc_name/{mainAccount.Id}")
                    .AddButtonDownIf(() => trueAcc != default, $"👤 Есть настоящий аккаунт", $"/account/{trueAcc?.Id}")
                    .AddButtonDownIf(() => trueAcc == default && accs.Any(), $"♻️ Объединить аккаунты ({accs.Count()} шт.)", $"/try_union_accounts/{mainAccount.Id}")
                    .AddButtonDown("Назад", $"/get_accounts/{Package.Account.GetAccountsPage}")
            );
        }

        [ApiPointer("account_books")]
        private string AccountBooks(Account mainAccount)
        {
            var books = BookService.GetBooks(mainAccount).GroupBy(x => x.ActualBookStartTime.Date).OrderBy(x => x.Key).ToArray();
            var constructor = new InlineKeyboardConstructor();
            foreach (var book in books)
            {
                if (book.Count() > 1)
                {
                    constructor.AddButtonDown($"{book.Key.ToShortDateString()} ({book.Count()} шт.)", $"/abbd/{mainAccount.Id}/{book.Key.Date}");
                }
                else
                {
                    constructor.AddButtonDown($"{book.Key.ToShortDateString()} {book.Single().GetTitle()}", $"/ab_show_book/{book.Single().Id}");
                }
            }

            return SendOrEdit(
                $"{mainAccount.GetCard()}" +
                $"\r\nИстория броней:",
                constructor
                    .AddButtonDown("Назад", $"/account/{mainAccount.Id}")
            );
        }

        /// <summary>
        /// Карточка брони для просмотра истории
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        [ApiPointer("ab_show_book")]
        private string AccountBooksShowBook(Book book)
        {
            return SendOrEdit(
                book.GetState(),
                book.GetButtons()
                    .AddButtonDown("Назад", $"/account_books/{book.Guest.Id}")
            );
        }

        [ApiPointer("abbd")]
        private string AccountBooksByDate(Account mainAccount, DateTime date)
        {
            var books = BookService.GetBooks(mainAccount)
                .Where(x => x.ActualBookStartTime.Date == date.Date)
                .OrderBy(x => x.ActualBookStartTime)
                .ToArray();

            return SendOrEdit(
                $"{mainAccount.GetCard()}" +
                $"\r\nНесколько броней на {date.ToddMM()}",
                new InlineKeyboardConstructor()
                    .AddHostesBooksButtons(books, "/abbd_show_book")
                    .AddButtonDown("Назад", $"/account_books/{mainAccount.Id}")
            );
        }

        /// <summary>
        /// Карточка брони, для просмотра истории, когда за день было несколько броней
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        [ApiPointer("abbd_show_book")]
        private string ABBD_ShowBook(Book book)
        {
            return SendOrEdit(
                book.GetState(),
                book.GetButtons()
                    .AddButtonDown("Назад", $"/abbd/{book.Guest.Id}/{book.ActualBookStartTime.Date}")
            );
        }

        [ApiPointer("try_union_accounts")]
        private string TryUnionAccounts(Account mainAccount)
        {
            var accs = AccountService.GetAccountsToUnion(mainAccount);
            return SendOrEdit(
                $"{mainAccount.GetCard()}" +
                $"\r\nОбъединить со следующими гостями?",
                new InlineKeyboardConstructor()
                    .AddHostesShowAccounts(accs, "account")
                    .AddButtonDown("Объединить", $"/union_accounts/{mainAccount.Id}")
                    .AddButtonDown("Назад", $"/account/{mainAccount.Id}")
            );
        }

        [ApiPointer("union_accounts")]
        private string UnionAccounts(Account mainAccount)
        {
            LogService.LogEvent(nameof(UnionAccounts) + ":" + mainAccount?.Id);
            var subAccounts = AccountService.GetAccountsToUnion(mainAccount);
            if (mainAccount.Phone == default)
            {
                mainAccount.Phone = subAccounts.Where(x => x.Phone != default).OrderBy(x => x.TimeStamp).LastOrDefault().Phone;
            }
            if (mainAccount.CardNumber == default)
            {
                mainAccount.CardNumber = subAccounts.Where(x => x.CardNumber != default).OrderBy(x => x.TimeStamp).LastOrDefault().CardNumber;
            }
            AccountService.AccountDS.Save(mainAccount);

            foreach (var subAccount in subAccounts)
            {
                var books = BookService.BookDS.GetAll().Where(x => x.Guest.Id == subAccount.Id).ToList();
                books.ForEach(x =>
                {
                    x.Guest = mainAccount;
                    BookService.BookDS.Save(x);
                });
                AccountService.AccountDS.Delete(subAccount);
            }

            return SendOrEdit(
                $"Аккаунты гостя {mainAccount} успешно объеденены\\!",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Назад", $"/account/{mainAccount.Id}")
            );
        }

        [ApiPointer("select_phone")]
        private string SelectPhone(Account mainAccount)
        {
            Package.Account.SelectedAccount = mainAccount;
            Package.Account.Waiting = WaitingText.PhoneNumber;
            AccountService.AccountDS.Save(Package.Account);

            return SendOrEdit(
                $"Укажите номер телефона гостя:" +
                $"\r\n{mainAccount.GetCard()}",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Назад", $"/account/{mainAccount.Id}")
            );
        }

        [ApiPointer("set_phone")]
        private string SetPhone(string phone)
        {
            Account selectedAccount = Package.Account.SelectedAccount;
            LogService.LogEvent(nameof(SetPhone) + ":" + selectedAccount?.Id + ":" + phone);
            if (selectedAccount == default)
            {
                return GetAccounts();
            }
            Package.Account.Waiting = WaitingText.None;
            Package.Account.SelectedAccount = default;
            AccountService.AccountDS.Save(Package.Account);

            selectedAccount.Phone = AccountService.ParseNumber(phone);
            AccountService.AccountDS.Save(selectedAccount);
            return Account(selectedAccount);
        }

        [ApiPointer("select_card")]
        private string SelectCard(Account mainAccount)
        {
            Package.Account.SelectedAccount = mainAccount;
            Package.Account.Waiting = WaitingText.CardNumber;
            AccountService.AccountDS.Save(Package.Account);

            return SendOrEdit(
                $"Укажите номер карты гостя:" +
                $"\r\n{mainAccount.GetCard()}",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Назад", $"/account/{mainAccount.Id}")
            );
        }

        [ApiPointer("set_card")]
        private string SetCard(string card)
        {
            Account selectedAccount = Package.Account.SelectedAccount;
            LogService.LogEvent(nameof(SetCard) + ":" + selectedAccount?.Id + ":" + card);
            if (selectedAccount == default)
            {
                return GetAccounts();
            }
            Package.Account.Waiting = WaitingText.None;
            Package.Account.SelectedAccount = default;
            AccountService.AccountDS.Save(Package.Account);

            selectedAccount.CardNumber = Package.Account != AccountService.MainTester && selectedAccount == AccountService.MainTester
                ? selectedAccount.CardNumber
                : AccountService.ParseNumber(card);
            AccountService.AccountDS.Save(selectedAccount);
            return Account(selectedAccount);
        }

        [ApiPointer("select_acc_name")]
        private string SelectAccName(Account mainAccount)
        {
            Package.Account.SelectedAccount = mainAccount;
            Package.Account.Waiting = WaitingText.AccName;
            AccountService.AccountDS.Save(Package.Account);

            return SendOrEdit(
                $"Укажите имя гостя:" +
                $"\r\n{mainAccount.GetCard()}",
                new InlineKeyboardConstructor()
                    .AddButtonDown("Назад", $"/account/{mainAccount.Id}")
            );
        }

        [ApiPointer("set_acc_name")]
        private string SetAccName(string name)
        {
            Account selectedAccount = Package.Account.SelectedAccount;
            LogService.LogEvent(nameof(SetAccName) + ":" + selectedAccount?.Id + ":" + name);
            if (selectedAccount == default)
            {
                return GetAccounts();
            }
            Package.Account.Waiting = WaitingText.None;
            Package.Account.SelectedAccount = default;
            AccountService.AccountDS.Save(Package.Account);

            selectedAccount.Name = name;
            AccountService.AccountDS.Save(selectedAccount);
            return Account(selectedAccount);
        }
        #endregion
    }
}
