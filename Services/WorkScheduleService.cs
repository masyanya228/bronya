using Bronya.DI;
using Bronya.DomainServices.DomainStructure;
using Bronya.Entities;
using Bronya.Enums;
using Bronya.Xtensions;

using Buratino.Xtensions;


namespace Bronya.Services
{
    public class WorkScheduleService
    {
        public IDomainService<WorkSchedule> WorkScheduleDS {  get; set; }

        public WorkScheduleService(Account account)
        {
            WorkScheduleDS = Container.GetDomainService<WorkSchedule>(account);
        }

        public WorkSchedule GetWorkSchedule(DateTime dateTime = default)
        {
            if (dateTime == default)
            {
                dateTime = new TimeService().GetNow();
            }

            var oneTimeSchedule = GetOneTimeSchedule(dateTime);
            if (oneTimeSchedule != default)
                return oneTimeSchedule;
            
            var standartSchedule = GetStandartSchedule(dateTime);
            if (standartSchedule != default)
                return standartSchedule;
            
            throw new ArgumentOutOfRangeException($"Для {dateTime:f} нет графика работы");
        }

        public WorkSchedule GetStandartSchedule(DateTime dateTime = default)
        {
            if (dateTime == default)
            {
                dateTime = new TimeService().GetNow();
            }

            var dayOfWeeks = dateTime.DayOfWeek.ToDayOfWeeks();
            var dayOfWeeksYesterday = dateTime.AddDays(-1).DayOfWeek.ToDayOfWeeks();
            var standartSchedules = WorkScheduleDS.GetAll(x => !x.IsOneTimeSchedule)
                .Where(x => x.DayOfWeeks.HasFlag(dayOfWeeks) || x.DayOfWeeks.HasFlag(dayOfWeeksYesterday))
                .OrderByDescending(x => x.StartDate)
                .ToArray();
            for (var i = 0; i < standartSchedules.Length; i++)
            {
                var workSchedule = standartSchedules[i];
                var prev = i + 1 < standartSchedules.Length
                    ? standartSchedules[i + 1]
                    : null;

                if (workSchedule.StartDate > dateTime.Date)
                {
                    continue;
                }

                if (prev != null)
                {
                    if (dateTime.Date >= workSchedule.StartDate && dateTime.Date.AddDays(-1).Add(prev.Start).Add(prev.Length) > dateTime)
                    {
                        return prev;
                    }
                    else
                    {
                        return workSchedule;
                    }
                }
                else
                {
                    return workSchedule;
                }
            }
            return null;
        }

        public WorkSchedule GetOneTimeSchedule(DateTime dateTime = default)
        {
            if (dateTime == default)
            {
                dateTime = new TimeService().GetNow();
            }

            var oneTimeSchedules = WorkScheduleDS.GetAll(x => x.IsOneTimeSchedule).OrderByDescending(x => x.StartDate).ToArray();
            for (int i = 0; i < oneTimeSchedules.Length; i++)
            {
                var workSchedule = oneTimeSchedules[i];
                if (workSchedule.IsOneTimeSchedule
                    && dateTime.Between_LTE_GTE(workSchedule.StartDate.Add(workSchedule.Start), workSchedule.StartDate.Add(workSchedule.Start).Add(workSchedule.Length)))
                {
                    return workSchedule;
                }
            }
            return null;
        }
    }
}
