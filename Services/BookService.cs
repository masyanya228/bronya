using Bronya.Dtos;
using Bronya.Entities;

using Buratino.DI;
using Buratino.Models.DomainService.DomainStructure;

namespace Bronya.Services
{
    public class BookService
    {

        public IDomainService<Book> BookDS { get; set; }
        public IDomainService<Table> TableDS {  get; set; }
        public WorkScheduleService ScheduleService {  get; set; }
        
        private SmenaDto smena;
        public SmenaDto Smena
        {
            get
            {
                if (smena == null)
                {
                    smena = GetCurrentSmena();
                }
                return smena;
            }
        }

        public BookService(Account account)
        {
            BookDS = Container.GetDomainService<Book>(account);
            TableDS = Container.GetDomainService<Table>(account);
            ScheduleService = new(account);
        }

        public TimeService TimeService { get; set; } = new TimeService();

        public DateTime[] GetAvailableTimesForMove(Table table, Account acc)
        {
            List<DateTime> times = new();

            List<Book> books = GetCurrentBooks(table);

            for (var i = Smena.GetMinimumTimeToBook(acc); i <= Smena.SmenaEnd; i = i.Add(Smena.Schedule.Step))
            {
                if (books.Any(x => i > x.ActualBookStartTime.Add(-Smena.Schedule.Buffer) && i < x.BookEndTime.Add(Smena.Schedule.Buffer)))
                {
                    continue;
                }
                else
                {
                    times.Add(i);
                }
            }
            return times.ToArray();
        }

        /// <summary>
        /// Возвращает открытые брони гостя на определенную смену
        /// </summary>
        /// <param name="account"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public IEnumerable<Book> GetMyActualBooks(Account account)
        {
            var books = GetCurrentBooks()
                .Where(x => x.Guest.Id == account.Id
                    && x.TableClosed == default)
                .ToList();
            return books;
        }

        public bool CanCancel(Book book)
        {
            return !book.IsCanceled && book.TableStarted == default && book.TableClosed == default;
        }

        public bool Cancel(Book book)
        {
            if (CanCancel(book))
            {
                book.IsCanceled = true;
                BookDS.Save(book);
                return true;
            }
            return false;
        }

        public bool CanMove(Book book, Account acc)
        {
            var times = GetAvailableTimesForMove(book.Table, acc);
            return times.Contains(book.BookEndTime.Add(Smena.Schedule.Step));
        }

        public bool Move(Book book, Account acc)
        {
            if (CanMove(book, acc))
            {
                book.ActualBookStartTime = GetTimeAfterMove(book);
                BookDS.Save(book);
                return true;
            }
            return false;
        }

        public bool CanRepair(Book book, Account acc)
        {
            return Smena.GetMinimumTimeToBook(acc) <= book.BookEndTime;
        }

        public DateTime GetTimeAfterMove(Book book)
        {
            return book.ActualBookStartTime.Add(Smena.Schedule.Step);
        }

        private SmenaDto GetCurrentSmena()
        {
            DateTime now = TimeService.GetNow();
            var workSchedule = ScheduleService.GetWorkSchedule(now);
            GetCurrentSmena(now, workSchedule, out DateTime smenaStart, out DateTime smenaEnd);
            return new SmenaDto()
            {
                Schedule = workSchedule,
                SmenaStart = smenaStart,
                SmenaEnd = smenaEnd,
            };
        }

        public DateTime[] GetAvailableTimesForBook(Account acc)
        {
            IEnumerable<DateTime> allTimes = Array.Empty<DateTime>();
            foreach (var table in TableDS.GetAll())
            {
                allTimes = allTimes.Union(GetAvailableTimesForBook(table, acc));
            }
            return allTimes.OrderBy(x => x).ToArray();
        }

        public DateTime[] GetProlongationVariants(Book book, Account acc)
        {
            var timesToProlongation = new List<DateTime>();
            var availableTimes = GetAvailableTimesForMove(book.Table, acc);
            var maxProlongationTime = book.GetTrueEndBook().Add(Smena.Schedule.MinPeriod) < Smena.SmenaEnd
                ? book.GetTrueEndBook().Add(Smena.Schedule.MinPeriod)
                : Smena.SmenaEnd;
            for (var i = book.BookEndTime.Add(Smena.Schedule.Step); i <= maxProlongationTime; i = i.Add(Smena.Schedule.Step))
            {
                if (!availableTimes.Contains(i))
                {
                    break;
                }
                timesToProlongation.Add(i);
            }
            return timesToProlongation.ToArray();
        }

        public DateTime[] GetMoveVariants(Book book, Account acc)
        {
            return GetAvailableTimesForBook(book.Table, acc, book);
        }

        /// <summary>
        /// Возвращает брони на текущую смену, на указанный стол
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public List<Book> GetCurrentBooks(Table table, bool exceptPast = false)
        {
            return GetCurrentBooks()
                .Where(x => x.Table.Id == table.Id)
                .Where(x => !exceptPast || x.GetTrueEndBook().Add(Smena.Schedule.MinPeriod) > TimeService.GetNow())
                .ToList();
        }

        /// <summary>
        /// Возвращает брони на текущую смену
        /// </summary>
        /// <param name="table"></param>
        /// <param name="smenaStart"></param>
        /// <param name="smenaEnd"></param>
        /// <returns></returns>
        public List<Book> GetCurrentBooks()
        {
            return BookDS.GetAll()
                .Where(x => x.ActualBookStartTime >= Smena.SmenaStart
                    && !x.IsCanceled)
                .ToList()
                .Where(x => x.BookEndTime <= Smena.SmenaEnd)
                .OrderBy(x => x.ActualBookStartTime)
                .ToList();
        }

        /// <summary>
        /// Возвращает все брони гостя
        /// </summary>
        /// <param name="mainAccount"></param>
        /// <returns></returns>
        public IEnumerable<Book> GetBooks(Account mainAccount)
        {
            return BookDS.GetAll().Where(x => x.Guest.Id == mainAccount.Id).ToArray();
        }

        /// <summary>
        /// Возвращает временные границы текущей смены
        /// </summary>
        /// <param name="now"></param>
        /// <param name="workSchedule"></param>
        /// <param name="smenaStart"></param>
        /// <param name="smenaEnd"></param>
        private void GetCurrentSmena(DateTime now, WorkSchedule workSchedule, out DateTime smenaStart, out DateTime smenaEnd)
        {
            var smenaDay = now.Date;
            if (now.Date.AddDays(-1).Add(workSchedule.Start).Add(workSchedule.Length) > now)
                smenaDay = smenaDay.AddDays(-1);
            smenaStart = smenaDay.Add(workSchedule.Start);
            smenaEnd = smenaStart.Add(workSchedule.Length);
        }

        public DateTime[] GetAvailableTimesForBook(Table table, Account acc, Book except = default, List<Book> tableBooks = default)
        {
            if (!table.IsBookAvailable)
                return Array.Empty<DateTime>();
            List<DateTime> times = new();

            if (tableBooks == default)
            {
                tableBooks = GetCurrentBooks(table);
            }
            List<Book> books = tableBooks.Except([except]).Where(x => x.TableClosed == default).ToList();

            for (var i = Smena.GetMinimumTimeToBook(acc); i <= Smena.SmenaEnd.Subtract(Smena.Schedule.MinPeriod); i = i.Add(Smena.Schedule.Step))
            {
                if (books.Any(x => i > x.ActualBookStartTime.Add(-Smena.Schedule.MinPeriod).Add(-Smena.Schedule.Buffer) && i < x.BookEndTime.Add(Smena.Schedule.Buffer)))
                {
                    continue;
                }
                else
                {
                    times.Add(i);
                }
            }
            return times.ToArray();
        }
    }
}
