using Bronya.Entities;
using Bronya.Services;

using Buratino.DI;
using Buratino.Helpers;

using FluentNHibernate.Conventions;

namespace Buratino.Xtensions
{
    public static class KeyboardConstructorXtensions
    {
        private static BookService bookService = new BookService();

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
        public static InlineKeyboardConstructor AddTableButtons(this InlineKeyboardConstructor constructor)
        {
            var tables = Container.GetDomainService<Table>().GetAll().Where(x => x.IsBookAvailable).ToArray();
            int count = 0;
            int tablesInRow = 3;
            foreach (var table in tables)
            {
                bool isBusy = !bookService.GetAvailableTimesForBook(table).Any();
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

        public static InlineKeyboardConstructor AddAllHostesTableButtons(this InlineKeyboardConstructor constructor)
        {
            var tables = Container.GetDomainService<Table>().GetAll().OrderBy(x => x.Number).ToArray();
            int count = 0;
            int tablesInRow = 3;
            foreach (var table in tables)
            {
                var books = bookService.GetCurrentBooks(table);
                bool isBusy = !bookService.GetAvailableTimesForBook(table).Any();
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

        public static InlineKeyboardConstructor AddHostesTableButtons(this InlineKeyboardConstructor constructor, IEnumerable<Table> tables)
        {
            int count = 0;
            int tablesInRow = 3;
            foreach (var table in tables)
            {
                var books = bookService.GetCurrentBooks(table);
                bool isBusy = !bookService.GetAvailableTimesForBook(table).Any();
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
                constructor.AddButtonDown($"{item.ActualBookStartTime:dd.MM HH:mm} {item.Account.ToString()} Гостей:{item.SeatAmount}", $"/show_book/{item.Id}");
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
    }
}
