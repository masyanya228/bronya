using Bronya.Dtos;
using Bronya.Entities;
using Bronya.Enums;

using Buratino.Helpers;
using Buratino.Xtensions;

using vkteams.Xtensions;

namespace Bronya.Services
{
    public class StatisticService
    {
        public void SendStats()
        {
            LogService logService = new LogService(AccountService.RootAccount);
            BookService bookService = new BookService(AccountService.RootAccount);
            var now = new TimeService().GetNow();
            var yesterday = now.AddDays(-1);
            var smena = bookService.GetCurrentSmena(yesterday);
            var books = bookService.GetBooks(smena);

            AuthorizeService authorizeService = AuthorizeService.Instance;
            var admins = authorizeService.AccountService.AccountDS.GetAll()
                .Where(x => x.TGChatId == "564244276")
                .Append(authorizeService.AccountService.AccountDS.Get(AccountService.MainTester.Id))
                .ToList();

            foreach (var admin in admins)
            {
                authorizeService.TgAPI.SendOrEdit(
                    new DataPackage(admin),
                    $"За вчерашную смену было {books.Count.TrueNumbers("бронь", "брони", "броней")}"
                );
                logService.LogEvent(nameof(SendStats) + ":" + admin?.Id);
            }
        }
    }
}
