using Bronya.Enums;

namespace Bronya.Services
{
    public class BookAutoCloselService
    {
        public void CloseBooks()
        {
            LogService logService = new LogService(AccountService.RootAccount);
            BookService bookService = new BookService(AccountService.RootAccount);
            var now = new TimeService().GetNow();
            var smena = bookService.Smena;

            if (smena.SmenaStart.Subtract(now).TotalHours > 1)//Если до старта смены осталось меньше часа
                return;

            var booksToCancel = bookService.GetCurrentBooks()
                .Where(x => x.GetStatus() == BookStatus.Opened)
                .ToList();

            foreach (var book in booksToCancel)
            {
                book.TableClosed = bookService.GetCurrentSmena(book.TableAllowedStarted).SmenaEnd;
                book.Comment += $"Бронь автоматически закрыта, так как её не закрыли на прошлой смене";
                bookService.BookDS.Save(book);
                logService.LogEvent(nameof(CloseBooks) + ":" + book?.Id);
            }
        }
    }
}
