using Bronya.Entities;
using Bronya.Enums;
using Bronya.Services;

namespace Bronya.Dtos
{
    public class SmenaDto
    {
        public DateTime SmenaStart { get; set; }

        public DateTime SmenaEnd { get; set; }

        public DateTime GetMinimumTimeToBook(Account account)
        {
            var now = new TimeService().GetNow();
            var correctTime = now.Date.AddHours(now.Hour);
            while (correctTime <= now)
            {
                correctTime = correctTime.Add(Schedule.Step);
            }
            if (account != default && AuthorizeService.Instance.GetRole(account) == RoleType.Hostes)
            {
                correctTime = correctTime.Add(-Schedule.Step);
            }
            return SmenaStart > correctTime
                ? SmenaStart
                : correctTime;
        }

        public DateTime GetMaximumTimeToBook(Account account, SmenaDto smena)
        {
            return (account?.Roles?.Count ?? 0) > 0
                ? smena.SmenaEnd.Subtract(smena.Schedule.Step * 2)
                : smena.SmenaEnd.Subtract(smena.Schedule.MinPeriod);
        }

        public WorkSchedule Schedule { get; set; }
    }
}
