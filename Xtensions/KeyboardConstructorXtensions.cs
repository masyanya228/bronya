﻿using Bronya.Entities;
using Bronya.Enums;
using Bronya.Services;

using Buratino.DI;
using Buratino.Entities;
using Buratino.Helpers;

using Telegram.Bot.Types.ReplyMarkups;

namespace Buratino.Xtensions
{
    public static class KeyboardConstructorXtensions
    {
        private static BookService bookService = new BookService();

        public static IEnumerable<IEnumerable<InlineKeyboardButton>> ToGrid<T>(this IEnumerable<T> filters, Func<T, string> titleSelector, Func<T, string> callbackSelector, int columnCount = 2)
        {
            return filters.Chunk(columnCount).Select(x => x.Select(y => new InlineKeyboardButton(titleSelector(y)) { CallbackData = callbackSelector(y) }));
        }

        public static InlineKeyboardConstructor AddButtonDownIf(this InlineKeyboardConstructor constructor, Func<bool> func, string title, string callbackQuery)
            => func()
                ? constructor.AddButtonDown(title, callbackQuery)
                : constructor;

        public static InlineKeyboardConstructor AddButtonRightIf(this InlineKeyboardConstructor constructor, Func<bool> func, string title, string callbackQuery)
            => func()
                ? constructor.AddButtonRight(title, callbackQuery)
        : constructor;

        /// <summary>
        /// Отображение столов для гостя
        /// </summary>
        /// <param name="constructor"></param>
        /// <returns></returns>
        public static InlineKeyboardConstructor AddTableButtons(this InlineKeyboardConstructor constructor, Entities.Account acc)
        {
            var tables = Container.GetDomainService<Table>().GetAll().Where(x => x.IsBookAvailable).ToArray();
            int count = 0;
            int tablesInRow = 3;
            foreach (var table in tables)
            {
                bool isBusy = !bookService.GetAvailableTimesForBook(table, acc).Any();
                var btnTitle = isBusy
                    ? $"🔒 {table.Name}"
                    : $"{table.Name}";

                if (count == tablesInRow)
                {
                    count = 0;
                    constructor.AddButtonDown(btnTitle, $"/table/{table.Name}");
                }
                else
                {
                    constructor.AddButtonRight(btnTitle, $"/table/{table.Name}");
                }
                count++;
            }
            return constructor;
        }

        /// <summary>
        /// Отображение времени для гостя
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="avalableTimes"></param>
        /// <returns></returns>
        public static InlineKeyboardConstructor AddTimeButtons(this InlineKeyboardConstructor constructor, DateTime[] avalableTimes)
        {
            DateTime prevTime = DateTime.MinValue;
            foreach (var i in avalableTimes)
            {
                if (i.Minute == 0 || prevTime.Hour != i.Hour)
                    constructor.AddButtonDown($"{i.Hour:00}:{i.Minute:00}", $"/set_time/{i}");
                else
                    constructor.AddButtonRight($"{i.Hour:00}:{i.Minute:00}", $"/set_time/{i}");
                prevTime = i;
            }
            return constructor;
        }

        /// <summary>
        /// Отображение посадочных мест для гостя
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static InlineKeyboardConstructor AddPlacesButtons(this InlineKeyboardConstructor constructor, Table table)
        {
            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            for (var i = 1; i <= table.NormalSeatAmount; i++)
            {
                constructor.AddButtonDown(i.ToString(), $"/places/{i}");
            }
            constructor.AddButtonDown($"{table.NormalSeatAmount}+", "/morethan");
            return constructor;
        }

        /// <summary>
        /// Отображение броней для гостя
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="books"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static InlineKeyboardConstructor AddBooksButtons(this InlineKeyboardConstructor constructor, IEnumerable<Book> books)
        {
            if (books is null)
            {
                throw new ArgumentNullException(nameof(books));
            }

            foreach (var item in books)
            {
                constructor.AddButtonDown($"{item.ActualBookStartTime:dd.MM HH:mm} Стол: {item.Table.Name} Гостей:{item.SeatAmount}", $"/mybook/{item.Id}");
            }
            return constructor;
        }

        public static InlineKeyboardConstructor AddHostesAllTableButtons(this InlineKeyboardConstructor constructor, Entities.Account acc)
        {
            var tables = Container.GetDomainService<Table>().GetAll().OrderBy(x => x.Number).ToArray();
            int count = 0;
            int tablesInRow = 2;
            foreach (var table in tables)
            {
                var books = bookService.GetCurrentBooks(table);
                bool isBusy = !bookService.GetAvailableTimesForBook(table, acc).Any();
                var btnTitle = isBusy
                    ? $"🔒 {table.Name}"
                    : $"{table.Name}";

                if (books.Any())
                    btnTitle += $" \r\n📘{books.Count}";

                if (!table.IsBookAvailable)
                    btnTitle = $"🚫 {table.Name}";

                if (count == tablesInRow)
                {
                    count = 0;
                    constructor.AddButtonDown(btnTitle, $"/table/{table.Id}");
                }
                else
                {
                    constructor.AddButtonRight(btnTitle, $"/table/{table.Id}");
                }
                count++;
            }
            return constructor;
        }

        public static InlineKeyboardConstructor AddHostesNowTableButtons(this InlineKeyboardConstructor constructor)
        {
            var now = new TimeService().GetNow();
            var tables = Container.GetDomainService<Table>().GetAll().OrderBy(x => x.Number).ToArray();
            var smena = bookService.GetCurrentSmena();
            int count = 0;
            int tablesInRow = 1;
            foreach (var table in tables)
            {
                var books = bookService.GetCurrentBooks(table);

                var actualBook = books.FirstOrDefault(x => x.GetTrueStartBook() < now && x.GetTrueEndBook() > now && x.GetStatus() != BookStatus.Closed);
                var nowOpened = actualBook != default
                    ? actualBook.TableStarted != default && actualBook.TableClosed == default
                    : false;
                var nowNotOpened = actualBook != default
                    ? actualBook.TableStarted == default
                    : false;
                var allowToBookNow = now.Add(smena.Schedule.MinPeriod) < smena.SmenaEnd && books.All(x => !x.IsIntersected(now, now.Add(smena.Schedule.MinPeriod)));

                var isCloseToEnd = actualBook != default
                    ? actualBook.NotifiedAboutEndBook != default
                    : false;

                string btnTitle = string.Empty;
                string btnCallback = string.Empty;
                if (!table.IsBookAvailable)
                    btnTitle += $"🚫";

                if (isCloseToEnd)
                {
                    btnTitle += $"Время подходит к концу {table.Name}";
                    btnCallback = $"/show_book/{actualBook.Id}";
                }
                else if (nowOpened)
                {
                    btnTitle += $"Открыт {table.Name}";
                    btnCallback = $"/show_book/{actualBook.Id}";
                }
                else if (nowNotOpened)
                {
                    btnTitle += $"Не открыт {table.Name}";
                    btnCallback = $"/show_book/{actualBook.Id}";
                }
                else if (allowToBookNow)
                {
                    btnTitle += $"Свободно {table.Name}";
                    btnCallback = $"/table/{table.Id}";
                }
                else
                {
                    btnTitle += $"Нельзя {table.Name}";
                    btnCallback = $"/table/{table.Id}";
                }

                if (count == tablesInRow)
                {
                    count = 0;
                    constructor.AddButtonDown(btnTitle, btnCallback);
                }
                else
                {
                    constructor.AddButtonRight(btnTitle, btnCallback);
                }
                count++;
            }
            return constructor;
        }

        public static InlineKeyboardConstructor AddHostesTableButtons(this InlineKeyboardConstructor constructor, IEnumerable<Table> tables, Entities.Account acc)
        {
            int count = 0;
            int tablesInRow = 2;
            foreach (var table in tables)
            {
                var books = bookService.GetCurrentBooks(table);
                bool isBusy = !bookService.GetAvailableTimesForBook(table, acc).Any();
                var btnTitle = isBusy
                    ? $"🔒 {table.Name}"
                    : $"{table.Name}";

                if (books.Any())
                    btnTitle += $" \r\n📘{books.Count}";

                if (!table.IsBookAvailable)
                    btnTitle = $"🚫 {table.Name}";

                if (count == tablesInRow)
                {
                    count = 0;
                    constructor.AddButtonDown(btnTitle, $"/set_table/{table.Id}");
                }
                else
                {
                    constructor.AddButtonRight(btnTitle, $"/set_table/{table.Id}");
                }
                count++;
            }
            return constructor;
        }

        public static InlineKeyboardConstructor AddHostesBooksButtons(this InlineKeyboardConstructor constructor, IEnumerable<Book> books)
        {
            if (books is null)
            {
                throw new ArgumentNullException(nameof(books));
            }

            foreach (var item in books)
            {
                constructor.AddButtonDown($"{item.ActualBookStartTime:HH:mm} {item.Account.ToString()} 👤:{item.SeatAmount}", $"/show_book/{item.Id}");
            }
            return constructor;
        }

        public static InlineKeyboardConstructor AddHostesPlacesButtons(this InlineKeyboardConstructor constructor, Table table)
        {
            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }
            int count = 0;
            int tablesInRow = 3;
            for (var i = 1; i <= table.NormalSeatAmount; i++)
            {
                if (count == tablesInRow)
                {
                    count = 0;
                    constructor.AddButtonDown($"✅{i}", $"/set_places/{i}");
                }
                else
                {
                    constructor.AddButtonRight($"✅{i}", $"/set_places/{i}");
                }
                count++;
                
            }
            for (var i = table.NormalSeatAmount + 1; i <= 12; i++)
            {
                if (count == tablesInRow)
                {
                    count = 0;
                    constructor.AddButtonDown($"{i}", $"/set_places/{i}");
                }
                else
                {
                    constructor.AddButtonRight($"{i}", $"/set_places/{i}");
                }
                count++;
                
            }
            return constructor;
        }

        public static InlineKeyboardConstructor AddHostesProlongationVariantsButtons(this InlineKeyboardConstructor constructor, Book book, DateTime[] times)
        {
            int count = 0;
            int tablesInRow = 1;
            foreach (var time in times)
            {
                var diff = time.Subtract(book.BookEndTime);
                string diffTitle = diff.TotalHours < 1
                    ? $"{diff.Minutes}мин."
                    : $"{diff.Hours}ч. {diff.Minutes}мин.";
                if (count == tablesInRow)
                {
                    count = 0;
                    constructor.AddButtonDown($"+ {diffTitle} до {time:HH:mm}", $"/pg/{book.Id}/{time}");
                }
                else
                {
                    constructor.AddButtonRight($"+ {diffTitle} до {time:HH:mm}", $"/pg/{book.Id}/{time}");
                }
                count++;
            }
            return constructor;
        }

        public static InlineKeyboardConstructor AddHostesMoveVariantsButtons(this InlineKeyboardConstructor constructor, Book book, DateTime[] times)
        {
            DateTime prevTime = DateTime.MinValue;
            foreach (var i in times)
            {
                if (i.Minute == 0 || prevTime.Hour != i.Hour)
                    constructor.AddButtonDown($"{i:HH:mm}", $"/move/{book.Id}/{i}");
                else
                    constructor.AddButtonRight($"{i:HH:mm}", $"/move/{book.Id}/{i}");
                prevTime = i;
            }
            return constructor;
        }

        public static InlineKeyboardConstructor AddHostesSelectAccounts(this InlineKeyboardConstructor constructor, IEnumerable<Account> accounts)
        {
            int count = 0;
            int tablesInRow = 1;
            foreach (var account in accounts)
            {
                if (count == tablesInRow)
                {
                    count = 0;
                    constructor.AddButtonDown($"{account.Name} ({account.CardNumber}) {account.Phone}", $"/set_name_true/{account.Id}");
                }
                else
                {
                    constructor.AddButtonRight($"{account.Name} ({account.CardNumber}) {account.Phone}", $"/set_name_true/{account.Id}");
                }
                count++;
            }
            return constructor;
        }

        public static InlineKeyboardConstructor AddHostesAllAccounts(this InlineKeyboardConstructor constructor, IEnumerable<Account> accounts)
        {
            int count = 0;
            int accountsInRow = 1;
            foreach (var account in accounts)
            {
                if (count == accountsInRow)
                {
                    count = 0;
                    constructor.AddButtonDown($"{account.Name} ({account.CardNumber}) {account.Phone}", $"/account/{account.Id}");
                }
                else
                {
                    constructor.AddButtonRight($"{account.Name} ({account.CardNumber}) {account.Phone}", $"/account/{account.Id}");
                }
                count++;
            }
            return constructor;
        }
    }
}
