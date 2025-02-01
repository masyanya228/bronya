using Quartz;

namespace Bronya.Jobs.Structures
{
    public interface IQuartzProvider
    {
        IScheduler Schedule { get; }
    }
}