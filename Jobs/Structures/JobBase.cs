using Quartz;

namespace Bronya.Jobs.Structures
{
    public abstract class JobBase : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                Execute();
            });
        }

        public abstract void Execute();
    }
}
