using Bronya.Entities;
using Bronya.Services;

using Buratino.DI;
using Buratino.Helpers;

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
                    constructor.AddButtonDown($"{i.Hour:00}:{i.Minute:00}", $"/time/{i}");
                else
                    constructor.AddButtonRight($"{i.Hour:00}:{i.Minute:00}", $"/time/{i}");
                prevTime = i;
            }
            return constructor;
        }

        /// <summary>
        /// Отображение посадочных мест для гостя
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="selectedTable"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static InlineKeyboardConstructor AddPlacesButtons(this InlineKeyboardConstructor constructor, Table selectedTable)
        {
            if (selectedTable is null)
            {
                throw new ArgumentNullException(nameof(selectedTable));
            }

            for (var i = 1; i <= selectedTable.NormalSeatAmount; i++)
            {
                constructor.AddButtonDown(i.ToString(), $"/places/{i}");
            }
            constructor.AddButtonDown($"{selectedTable.NormalSeatAmount}+", "/morethan");
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


        public static InlineKeyboardConstructor AddHostesTableButtons(this InlineKeyboardConstructor constructor)
        {
            var tables = Container.GetDomainService<Table>().GetAll().Where(x => x.IsBookAvailable).ToArray();
            int count = 0;
            int tablesInRow = 3;
            foreach (var table in tables)
            {
                var books = bookService.GetCurrentBooks(table);
                bool isBusy = !bookService.GetAvailableTimesForBook(table).Any();
                var btnTitle = isBusy
                    ? $"🔒 {table.Name} ({books.Count})"
                    : $"{table.Name} ({books.Count})";

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
    }
}
