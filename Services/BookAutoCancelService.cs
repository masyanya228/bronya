using Bronya.Enums;

namespace Bronya.Services
{
    public class BookAutoCancelService
    {
        public void CancelBooks()
        {
            LogService logService = new LogService(AccountService.RootAccount);
            BookService bookService = new BookService(AccountService.RootAccount);
            var now = new TimeService().GetNow();
            var smena = bookService.GetCurrentSmena();
            var booksToCancel = bookService.GetCurrentBooks()
                .Where(x => x.GetStatus() == BookStatus.Booked)
                .Where(x => x.Comment == default)
                .Where(x => now.Subtract(x.ActualBookStartTime) > smena.Schedule.AutoCancelBook)
                .ToList();
            foreach (var book in booksToCancel)
            {
                book.IsCanceled = true;
                book.Comment += $"Бронь автоматически отмена, так как прошло больше {smena.Schedule.AutoCancelBook}";
                bookService.BookDS.Save(book);
                logService.LogEvent(nameof(CancelBooks) + ":" + book?.Id);
            }
        }
    }
}
