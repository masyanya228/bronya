using Bronya.Enums;

namespace Bronya.Services
{
    public class BookEndNotifyService
    {
        public void Notify()
        {
            BookService bookService = new BookService();
            var smena = bookService.GetCurrentSmena();
            var now = new TimeService().GetNow();
            var booksToNotify = bookService.GetCurrentBooks()
                .Where(x => x.GetStatus() == BookStatus.Opened)
                .Where(x => x.NotifiedAboutEndBook == default)
                .Where(x => now.Subtract(x.BookEndTime.Add(-smena.Schedule.NotificationBeforeBookEnd)) > TimeSpan.Zero)
                .ToList();
            foreach (var book in booksToNotify)
            {
                book.NotifiedAboutEndBook = now;
                bookService.BookDS.Save(book);
            }
        }
    }
}
