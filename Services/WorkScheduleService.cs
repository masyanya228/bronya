using Bronya.Entities;
using Buratino.DI;
using Buratino.Models.DomainService.DomainStructure;
using Buratino.Xtensions;


namespace Bronya.Services
{
    public class WorkScheduleService
    {
        public IDomainService<WorkSchedule> WorkScheduleDS = Container.GetDomainService<WorkSchedule>();

        public WorkSchedule GetWorkSchedule(DateTime dateTime)
        {
            var allSchedules = WorkScheduleDS.GetAll().OrderByDescending(x => x.StartDate).ToArray();
            
            var oneTimeSchedules = allSchedules.Where(x => x.IsOneTimeSchedule).ToArray();
            for (int i = 0; i < oneTimeSchedules.Length; i++)
            {
                var workSchedule = allSchedules[i];
                if (workSchedule.IsOneTimeSchedule
                    && dateTime.Between_LTE_GTE(workSchedule.StartDate.Add(workSchedule.Start), workSchedule.StartDate.Add(workSchedule.Start).Add(workSchedule.Length)))
                {
                    return workSchedule;
                }
            }

            var standartSchedules = allSchedules.Where(x => !x.IsOneTimeSchedule).ToArray();
            for (var i = 0; i < standartSchedules.Length; i++)
            {
                var workSchedule = standartSchedules[i];
                var prev = i < standartSchedules.Length
                    ? standartSchedules[i + 1]
                    : null;

                if (workSchedule.StartDate > dateTime.Date)
                {
                    continue;
                }

                if (prev != null)
                {
                    if (dateTime.Date.AddDays(-1).Add(prev.Start).Add(prev.Length) > dateTime)
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
            throw new ArgumentOutOfRangeException($"Для {dateTime:f} нет графика работы");
        }
    }
}
