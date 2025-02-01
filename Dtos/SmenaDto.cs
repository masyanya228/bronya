using Bronya.Entities;
using Bronya.Services;

using Buratino.Entities;

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
            if (AuthorizeService.Instance.IsHostes(account))
            {
                correctTime = correctTime.Add(-Schedule.Step);
            }
            return SmenaStart > correctTime
                ? SmenaStart
                : correctTime;
        }

        public WorkSchedule Schedule { get; set; }
    }
}
