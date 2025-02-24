using Bronya.Services;

using Quartz;

namespace Bronya.Jobs.Structures
{
    public abstract class JobBase : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                return Task.Factory.StartNew(() =>
                    {
                        Execute();
                    });
            }
            catch (Exception ex)
            {
                new ExceptionLogService().LogEvent(AccountService.RootAccount, ex);
                throw;
            }
        }

        public abstract void Execute();
    }
}
