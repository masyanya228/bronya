using Bronya.Services;
using Bronya.Xtensions;

using Quartz;

namespace Bronya.Jobs.Structures
{
    public abstract class JobBase : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                ConsoleXtensions.ColoredPrint($"Job: {GetType().Name}", ConsoleColor.Yellow);
                var result = Task.Factory.StartNew(Execute);
                result.Wait();
                ConsoleXtensions.ColoredPrint($"End: {GetType().Name}", ConsoleColor.Yellow);
                return result;
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
