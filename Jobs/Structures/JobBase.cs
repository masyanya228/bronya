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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Job");
                Console.ResetColor();
                var result = Task.Factory.StartNew(Execute);
                result.Wait();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("End");
                Console.ResetColor();
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
