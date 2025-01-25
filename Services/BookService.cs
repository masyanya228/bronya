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

        public DateTime[] GetAvailableTimes(Table table)
        {
            return GetAvailableTimes(table, TimeService.GetNow());
        }

        /// <summary>
        /// Возвращает открытые брони гостя на определенную смену
        /// </summary>
        /// <param name="account"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public IEnumerable<Book> GetMyActualBook(Account account)
        {
            return GetMyActualBook(account, TimeService.GetNow());
        }

        private IEnumerable<Book> GetMyActualBook(Account account, DateTime now)
        {
            var workSchedule = ScheduleService.GetWorkSchedule(now);
            now = RoundTime(now);

            DateTime smenaStart, smenaEnd;
            GetCurrentSmena(now, workSchedule, out smenaStart, out smenaEnd);
            var books = GetCurrentBooks(smenaStart, smenaEnd)
                .Where(x => x.Account.Id == account.Id
                    && x.TableClosed != default)
                .ToList();
            return books;
        }

        private DateTime[] GetAvailableTimes(Table table, DateTime now)
        {
            var workSchedule = ScheduleService.GetWorkSchedule(now);
            return GetAvailableTimes(table, now, workSchedule);
        }

        private DateTime[] GetAvailableTimes(Table table, DateTime now, WorkSchedule workSchedule)
        {
            List<DateTime> times = new();
            now = RoundTime(now);//Временное решение, когда текущее время округляется до часов, в меньшую сторону. Переделать на округление с шагом step, в меньшую сторону

            DateTime smenaStart, smenaEnd;
            GetCurrentSmena(now, workSchedule, out smenaStart, out smenaEnd);
            DateTime minStartTime = GetMinimumBookTime(now, smenaStart);
            List<Book> books = GetCurrentBooks(table, smenaStart, smenaEnd);

            for (var i = minStartTime; i <= smenaEnd.Subtract(workSchedule.MinPeriod); i = i.Add(workSchedule.Step))
            {
                if (books.Any(x => i > x.ActualBookStartTime.AddHours(-2).Add(-workSchedule.Buffer) && i < x.BookEndTime.Add(workSchedule.Buffer)))
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
                .Where(x => x.ActualBookStartTime >= smenaStart && x.BookEndTime <= smenaEnd
                    && !x.IsCanceled)
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
