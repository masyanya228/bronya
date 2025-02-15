using Buratino.API;
using Buratino.Attributes;
using Buratino.Xtensions;
using Buratino.Helpers;
using Bronya.Entities;
using Bronya.Services;
using Buratino.Enums;

namespace vkteams.Services
{
    /// <summary>
    /// Сервис взаимодействия с пользователем через telegram
    /// </summary>
    public class BronyaHostesService : BronyaServiceBase
    {
        public BronyaHostesService(LogService logService, TGAPI tGAPI, Account account) : base(logService, tGAPI, account)
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
                    .AddButtonDown("👤Гости", "/get_accounts")
                    .AddButtonDownIf(() => Package.Account.Id == new Guid("4be29f89-f887-48a1-a8af-cad15d032758"), "Роль", "/show_role")
                );
        }

        [ApiPointer("tables")]
        private string Tables()
        {
            return SendOrEdit(
                "Столы:",
                new InlineKeyboardConstructor()
                    .AddHostesAllTableButtons(Package.Account)
                    .AddButtonDown("Назад", $"/menu"),
                default,
                ImageId
                );
        }

        [ApiPointer("now")]
        private string Now()
        {
            return SendOrEdit(
                "Сейчас:",
                new InlineKeyboardConstructor()
                    .AddHostesNowTableButtons(Package.Account)
                    .AddButtonDown("Назад", $"/menu"),
                default,
                ImageId
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
                $"Стол: {table.Name.EscapeMarkdown1()}{isAvailable}",
                new InlineKeyboardConstructor()
                    .AddHostesBooksButtons(books)
                    .AddButtonDownIf(() => table.IsBookAvailable, "🚫 Отключить бронирование", $"/disable/{table.Id}")
                    .AddButtonDownIf(() => !table.IsBookAvailable, "✅ Включить бронирование", $"/enable/{table.Id}")
                    .AddButtonRight("➕📋", $"/book_select_time/{table.Id}")
                    .AddButtonDown("🔲 Столы", $"/tables")
                    .AddButtonRight("В начало", $"/menu"),
                default,
                stream
            );
        }

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
            else if (book.TableClosed != default)
            {
                return SendOrEdit(
                    book.GetState(),
                    new InlineKeyboardConstructor()
                        .AddButtonDownIf(() => book.Account.Phone == default, "Добавить телефон", $"/select_phone/{book.Account.Id}")
                        .AddButtonDownIf(() => book.Account.CardNumber == default, "Добавить карту", $"/select_card/{book.Account.Id}")
                        .AddButtonDown("↔️", $"/try_prolongate/{book.Id}")
                        .AddButtonRight("🔲", $"/table/{book.Table.Id}")
                        .AddButtonDown("✏️ Гость", $"/account/{book.Account.Id}")
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
                        .AddButtonRight("✅", $"/try_start_book/{book.Id}")
                        .AddButtonDown("🔴", $"/try_cancel/{book.Id}")
                        .AddButtonRight("⤵️", $"/try_move/{book.Id}")
                        .AddButtonRight("🔲", $"/table/{book.Table.Id}")
                        .AddButtonDown("В начало", $"/menu")
                    );
            }
        }

        [ApiPointer("try_move")]
        private string TryMove(Book book)
        {
            var vars = BookService.GetMoveVariants(book, Package.Account);
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
            BookService.BookDS.Save(book);
            return ShowBook(book);
        }

        [ApiPointer("try_prolongate")]
        private string TryProlongate(Book book)
        {
            var vars = BookService.GetProlongationVariants(book, Package.Account);
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
            book.NotifiedAboutEndBook = default;
            book.TableClosed = default;
            BookService.BookDS.Save(book);
            return ShowBook(book);
        }

        [ApiPointer("try_start_book")]
        private string TryStartBook(Book book)
        {
            var smena = BookService.GetCurrentSmena();
            var allReadyOpened = BookService
                .GetCurrentBooks(book.Table)
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
                string text = $"*Предыдущая бронь на имя {allReadyOpened.Account} не была закрыта. Проверьте себя.*";
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
            var smena = BookService.GetCurrentSmena();
            var allReadyOpened = BookService.GetCurrentBooks(book.Table)
                .FirstOrDefault(x => x != book && x.GetStatus() == Bronya.Enums.BookStatus.Opened);
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
                $"\r\n*Отменить бронь на {book.ActualBookStartTime:dd.MM HH:mm}?*",
                new InlineKeyboardConstructor()
                    .AddButtonDown("🔴Отменить🔴", $"/cancel/{book.Id}")
                    .AddButtonDown("Назад", $"/show_book/{book.Id}"));
        }

        [ApiPointer("cancel")]
        private string Cancel(Book book)
        {
            if (!BookService.Cancel(book))
            {
                return SendOrEdit(
                    $"*Бронь не получилось отменить*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/show_book/{book.Id}"));
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
            if (!BookService.CanRepair(book, Package.Account))
            {
                return SendOrEdit(
                    $"*Бронь не получилось восстановить*",
                    new InlineKeyboardConstructor()
                        .AddButtonDown("Назад", $"/show_book/{book.Id}"));
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
            book.TableClosed = new TimeService().GetNow();
            BookService.BookDS.Save(book);
            return ShowBook(book);
        }

        [ApiPointer("disable")]
        private string Disable(Table table)
        {
            table.IsBookAvailable = false;
            BookService.TableDS.Save(table);
            return Table(table);
        }

        [ApiPointer("enable")]
        private string Enable(Table table)
        {
            table.IsBookAvailable = true;
            BookService.TableDS.Save(table);
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
                ? BookService.GetAvailableTimesForBook(table, Package.Account)
                : BookService.GetAvailableTimesForBook(Package.Account);

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
                ? BookService.TableDS.GetAll().Where(x => x.IsBookAvailable).OrderBy(x => x.Number)
                    .ToArray()
                    .Where(table =>
                    {
                        var times = BookService.GetAvailableTimesForBook(table, Package.Account);
                        return times.Contains(Package.Account.SelectedTime);//Поиск по конкретному времени
                    }).ToArray()
                : BookService.TableDS.GetAll().Where(x => x.IsBookAvailable).OrderBy(x => x.Number)
                    .ToArray()
                    .Where(table =>
                    {
                        var times = BookService.GetAvailableTimesForBook(table, Package.Account);//Поиск столов, у которых есть свободное время
                        return times.Any();
                    }).ToArray();

            return SendOrEdit(
                    $"{Package.Account.GetNewBookState()}" +
                    $"\r\n*Выбор стола:*",
                    new InlineKeyboardConstructor()
                        .AddHostesTableButtons(tables, Package.Account)
                        .AddButtonDown("🗑", $"/reset_all")
                        .AddButtonRight(Package.Account.SelectedTime != default ? "✏️⏱️" : "✏️⏱️", "/book_select_time")
                        .AddButtonRightIf(() => Package.Account.SelectedTime != default, "♻️🔲", "/reset_time"),
                    default,
                    ImageId
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
                $"\r\n*Имя брони:*" +
                $"\r\n_Пример: Иван, 1111_",
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
            var smena = BookService.GetCurrentSmena();
            var newBook = new Book()
            {
                SeatAmount = Package.Account.SelectedPlaces,
                ActualBookStartTime = Package.Account.SelectedTime,
                BookLength = smena.Schedule.MinPeriod,
                Table = Package.Account.SelectedTable,
                Account = account
            };

            BookService.BookDS.Save(newBook);

            Package.Account.Waiting = default;
            Package.Account.SelectedPlaces = default;
            Package.Account.SelectedTable = default;
            Package.Account.SelectedTime = default;
            AccountService.AccountDS.Save(Package.Account);

            return ShowBook(newBook);
        }

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
                $"\r\nСтр. {page}/{maxPage}",
                new InlineKeyboardConstructor()
                    .AddHostesShowAccounts(accs.Skip((page - 1) * onPage).Take(onPage), "account")
                    .AddButtonDownIf(() => page > 1, "<", $"/get_accounts/{page - 1}")
                    .AddButtonDownIf(() => page < maxPage, ">", $"/get_accounts/{page + 1}")
                    .AddButtonDown("Назад", $"/menu")
            );
        }

        [ApiPointer("account")]
        private string Account(Account mainAccount)
        {
            Package.Account.SelectedAccount = default;
            Package.Account.Waiting = WaitingText.None;
            AccountService.AccountDS.Save(mainAccount);

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
            var books = BookService.GetBooks(mainAccount).GroupBy(x => x.ActualBookStartTime.Date).ToArray();

            var constructor = new InlineKeyboardConstructor();
            foreach (var book in books)
            {
                if (book.Count() > 1)
                {
                    constructor.AddButtonDown($"{book.Key.ToShortDateString()} ({book.Count()} шт.)", $"/abbd/{mainAccount.Id}/{book.Key.Date}");
                }
                else
                {
                    constructor.AddButtonDown($"{book.Key.ToShortDateString()}", $"/show_book/{book.Single().Id}");
                }
            }

            return SendOrEdit(
                $"{mainAccount.GetCard()}" +
                $"\r\nИстория броней:",
                constructor
                    .AddButtonDown("Назад", $"/account/{mainAccount.Id}")
            );
        }

        [ApiPointer("abbd")]
        private string AccountBooksByDate(Account mainAccount, DateTime date)
        {
            var books = BookService.GetBooks(mainAccount).Where(x => x.ActualBookStartTime.Date == date.Date).ToArray();

            return SendOrEdit(
                $"{mainAccount.GetCard()}" +
                $"\r\nНесколько броней на {date.ToShortDateString()}",
                new InlineKeyboardConstructor()
                    .AddHostesBooksButtons(books)
                    .AddButtonDown("Назад", $"/account_books/{mainAccount.Id}")
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
                var books = BookService.BookDS.GetAll().Where(x => x.Account.Id == subAccount.Id).ToList();
                books.ForEach(x =>
                {
                    x.Account = mainAccount;
                    BookService.BookDS.Save(x);
                });
                AccountService.AccountDS.Delete(subAccount);
            }

            return SendOrEdit(
                $"Аккаунты гостя {mainAccount} успешно объеденены!",
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
            if (selectedAccount == default)
            {
                return GetAccounts();
            }
            Package.Account.Waiting = WaitingText.None;
            Package.Account.SelectedAccount = default;
            AccountService.AccountDS.Save(Package.Account);
            
            selectedAccount.CardNumber = AccountService.ParseNumber(card);
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
    }
}
