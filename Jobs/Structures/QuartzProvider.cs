using Bronya.DI;

using Quartz;

namespace Bronya.Jobs.Structures
{
    public class QuartzProvider : IQuartzProvider
    {
        private IScheduler _Schedule;
        public IScheduler Schedule
        {
            get
            {
                if (_Schedule is null)
                {
                    _Schedule = Container.GetServiceProvider().GetRequiredService<ISchedulerFactory>().GetScheduler().GetAwaiter().GetResult();
                }
                return _Schedule;
            }
        }
    }
}
