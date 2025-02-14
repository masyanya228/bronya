using Bronya.Entities;
using Bronya.Enums;

namespace Bronya.Services
{
    public class BookAutoCancelService
    {
        public void CancelBooks()
        {
            var now = new TimeService().GetNow();
            BookService bookService = new BookService(new Account { Id = new Guid("da8c13be-6d97-4287-b47e-34caada8d315") });
            var smena = bookService.GetCurrentSmena();
            var booksToCancel = bookService.GetCurrentBooks()
                .Where(x => x.GetStatus() == BookStatus.Booked)
                .Where(x => now.Subtract(x.ActualBookStartTime) > smena.Schedule.AutoCancelBook)
                .ToList();
            foreach (var book in booksToCancel)
            {
                book.IsCanceled = true;
                book.Comment += $"Бронь автоматически отмена, так как прошло больше {smena.Schedule.AutoCancelBook}";
                bookService.BookDS.Save(book);
            }
        }
    }
}
