﻿using Bronya.Dtos;
using Bronya.Enums;
using Bronya.Helpers;

namespace Bronya.Services
{
    public class BookEndNotifyService
    {
        public void Notify()
        {
            LogService logService = new(AccountService.RootAccount);
            BookService bookService = new(AccountService.RootAccount);
            var smena = bookService.Smena;
            var now = new TimeService().GetNow();
            var booksToNotify = bookService.GetCurrentBooks()
                .Where(x => x.GetStatus() == BookStatus.Opened)
                .Where(x => x.NotifiedAboutEndBook == default)
                .Where(x => now.Subtract(x.TableAllowedStarted.Add(x.BookLength).Add(-smena.Schedule.NotificationBeforeBookEnd)) > TimeSpan.Zero)
                .ToList();

            AuthorizeService authorizeService = AuthorizeService.Instance;
            var hostesies = authorizeService.AccountService.AccountDS.GetAll().ToArray()
                .Where(x => authorizeService.GetRole(x) == RoleType.Hostes);
            foreach (var book in booksToNotify)
            {
                book.NotifiedAboutEndBook = now;
                bookService.BookDS.Save(book);
                logService.LogEvent(nameof(Notify) + ":" + book?.Id);
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
