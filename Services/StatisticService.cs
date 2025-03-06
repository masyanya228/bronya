using Bronya.Dtos;
using Bronya.Xtensions;

namespace Bronya.Services
{
    public class StatisticService
    {
        public void SendStats()
        {
            LogService logService = new(AccountService.RootAccount);
            BookService bookService = new(AccountService.RootAccount);
            var now = new TimeService().GetNow();
            var yesterday = now.AddDays(-1);
            var smena = bookService.GetCurrentSmena(yesterday);
            var books = bookService.GetBooks(smena);
            var booksByGuest = books.Where(x => x.Account.TGTag != "thegreenplacee").Count();

            AuthorizeService authorizeService = AuthorizeService.Instance;
            var admins = authorizeService.AccountService.AccountDS.GetAll()
                .Where(x => x.TGChatId == "564244276")
                .Append(authorizeService.AccountService.AccountDS.Get(AccountService.MainTester.Id))
                .ToList();

            var textMsg = $"За вчерашную смену было {books.Count.TrueNumbers("бронь", "брони", "броней")}";
            if (booksByGuest > 0)
                textMsg += $"\r\nИз них {booksByGuest} от гостей";

            foreach (var admin in admins)
            {
                authorizeService.TgAPI.SendOrEdit(
                    new DataPackage(admin),
                    textMsg
                );
                logService.LogEvent(nameof(SendStats) + ":" + admin?.Id);
            }
        }
    }
}
