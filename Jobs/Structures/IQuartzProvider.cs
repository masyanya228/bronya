using Quartz;

namespace Buratino.Jobs.Structures
{
    public interface IQuartzProvider
    {
        IScheduler Schedule { get; }
    }
}