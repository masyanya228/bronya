using Bronya.Dtos;
using Bronya.Entities;

using Buratino.DI;
using Buratino.Entities;
using Buratino.Models.DomainService.DomainStructure;

namespace Bronya.Services
{
    public class BookService
    {
        public IDomainService<Book> BookDS = Container.GetDomainService<Book>();
        public IDomainService<Table> TableDS = Container.GetDomainService<Table>();
        public WorkScheduleService ScheduleService = new WorkScheduleService();
        public TimeService TimeService = new TimeService();

        public DateTime[] GetAvailableTimesForBook(Table table)
        {
            return GetAvailableTimesForBook(table, TimeService.GetNow());
        }

        public DateTime[] GetAvailableTimesForMove(Table table)
        {
            return GetAvailableTimesForMove(table, TimeService.GetNow());
        }

        /// <summary>
        /// Возвращает открытые брони гостя на определенную смену
        /// </summary>
        /// <param name="account"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public IEnumerable<Book> GetMyActualBook(Account account)
        {
            return GetMyActualBooks(account, TimeService.GetNow());
        }

        private IEnumerable<Book> GetMyActualBooks(Account account, DateTime now)
        {
            var smena = GetCurrentSmena(now);
            var books = GetCurrentBooks(smena.SmenaStart, smena.SmenaEnd)
                .Where(x => x.Account.Id == account.Id
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

        public bool CanMove(Book book)
        {
            var times = GetAvailableTimesForMove(book.Table);
            var smena = GetCurrentSmena(TimeService.GetNow());
            return times.Contains(book.BookEndTime.Add(smena.Schedule.Step));
        }

        public bool Move(Book book)
        {
            if (CanMove(book))
            {
                book.ActualBookStartTime = GetTimeAfterMove(book);
                BookDS.Save(book);
                return true;
            }
            return false;
        }

        public DateTime GetTimeAfterMove(Book book)
        {
            var smena = GetCurrentSmena(TimeService.GetNow());
            return book.ActualBookStartTime.Add(smena.Schedule.Step);
        }

        public SmenaDto GetCurrentSmena()
        {
            DateTime now = TimeService.GetNow();
            var workSchedule = ScheduleService.GetWorkSchedule(now);
            now = RoundTime(now);
            DateTime smenaStart, smenaEnd;
            GetCurrentSmena(now, workSchedule, out smenaStart, out smenaEnd);
            return new SmenaDto()
            {
                Schedule = workSchedule,
                SmenaStart = smenaStart,
                SmenaEnd = smenaEnd,
                MinimumTimeToBook = GetMinimumBookTime(now, smenaStart),
            };
        }

        private SmenaDto GetCurrentSmena(DateTime now)
        {
            var workSchedule = ScheduleService.GetWorkSchedule(now);
            now = RoundTime(now);
            DateTime smenaStart, smenaEnd;
            GetCurrentSmena(now, workSchedule, out smenaStart, out smenaEnd);
            return new SmenaDto()
            {
                Schedule = workSchedule,
                SmenaStart = smenaStart,
                SmenaEnd = smenaEnd,
                MinimumTimeToBook = GetMinimumBookTime(now, smenaStart),
            };
        }

        private DateTime[] GetAvailableTimesForBook(Table table, DateTime now)
        {
            List<DateTime> times = new();

            var smena = GetCurrentSmena(now);
            List<Book> books = GetCurrentBooks(table, smena.SmenaStart, smena.SmenaEnd);

            for (var i = smena.MinimumTimeToBook; i <= smena.SmenaEnd.Subtract(smena.Schedule.MinPeriod); i = i.Add(smena.Schedule.Step))
            {
                if (books.Any(x => i > x.ActualBookStartTime.Add(-smena.Schedule.MinPeriod).Add(-smena.Schedule.Buffer) && i < x.BookEndTime.Add(smena.Schedule.Buffer)))
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

        private DateTime[] GetAvailableTimesForMove(Table table, DateTime now)
        {
            List<DateTime> times = new();

            var smena = GetCurrentSmena(now);
            List<Book> books = GetCurrentBooks(table, smena.SmenaStart, smena.SmenaEnd);

            for (var i = smena.MinimumTimeToBook; i <= smena.SmenaEnd; i = i.Add(smena.Schedule.Step))
            {
                if (books.Any(x => i > x.ActualBookStartTime.Add(-smena.Schedule.Buffer) && i < x.BookEndTime.Add(smena.Schedule.Buffer)))
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
        /// Округляет время
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        private DateTime RoundTime(DateTime now)//todo - Временное решение, когда текущее время округляется до часов, в меньшую сторону. Переделать на округление с шагом step, в меньшую сторону
        {
            now = now.Date.AddHours(now.Hour);
            return now;
        }

        /// <summary>
        /// Возвращает брони на текущую смену
        /// </summary>
        /// <param name="table"></param>
        /// <param name="smenaStart"></param>
        /// <param name="smenaEnd"></param>
        /// <returns></returns>
        private List<Book> GetCurrentBooks(DateTime smenaStart, DateTime smenaEnd)
        {
            return BookDS.GetAll()
                .Where(x => x.ActualBookStartTime >= smenaStart
                    && !x.IsCanceled)
                .ToList()
                .Where(x => x.ActualBookStartTime + x.BookLength <= smenaEnd)
                .ToList();
        }

        /// <summary>
        /// Возвращает брони на текущую смену, на указанный стол
        /// </summary>
        /// <param name="table"></param>
        /// <param name="smenaStart"></param>
        /// <param name="smenaEnd"></param>
        /// <returns></returns>
        private List<Book> GetCurrentBooks(Table table, DateTime smenaStart, DateTime smenaEnd)
        {
            return GetCurrentBooks(smenaStart, smenaEnd)
                .Where(x => x.Table.Id == table.Id)
                .ToList();
        }

        /// <summary>
        /// Возвращает самое раннее время для брони
        /// </summary>
        /// <param name="now"></param>
        /// <param name="smenaStart"></param>
        /// <returns></returns>
        private DateTime GetMinimumBookTime(DateTime now, DateTime smenaStart)
        {
            return smenaStart > now
                ? smenaStart
                : now;
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
            var smena = now.Date;
            if (now.Date.AddDays(-1).Add(workSchedule.Start).Add(workSchedule.Length) > now)
                smena = smena.AddDays(-1);
            smenaStart = smena.Add(workSchedule.Start);
            smenaEnd = smenaStart.Add(workSchedule.Length);
        }
    }
}
