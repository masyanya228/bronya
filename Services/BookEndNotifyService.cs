using Bronya.Dtos;
using Bronya.Enums;

using Buratino.Enums;
using Buratino.Helpers;

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

            AuthorizeService authorizeService = AuthorizeService.Instance;
            var hostesies = authorizeService.AccountService.AccountDS.GetAll().ToArray()
                .Where(x => authorizeService.GetRole(x) == RoleType.Hostes);
            foreach (var book in booksToNotify)
            {
                book.NotifiedAboutEndBook = now;
                bookService.BookDS.Save(book);
                foreach (var item in hostesies)
                {
                    authorizeService.TgAPI.SendOrEdit(
                        new DataPackage(item),
                        $"У стола {book.Table} заканчивается время",
                        new InlineKeyboardConstructor()
                            .AddButtonDown("📋 К брони", $"/show_book/{book.Id}")
                    );
                }
            }
        }
    }
}
