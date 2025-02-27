using BannerWebIS.Jobs;

using Buratino.DI;
using Buratino.Xtensions;

using Quartz;

namespace Bronya.Jobs.Structures
{
    public class JobRegistrator
    {
        public static IQuartzProvider QuartzProvider { get; set; } = Container.Get<IQuartzProvider>();

        public static void RegisterJobs()
        {
            new BookAutoCancelJob().RegisterJobWithCron();
            new BookEndNotifyJob().RegisterJobWithCron();
            new BookAutoCloselJob().RegisterJobWithCron();
            new StatisticJob().RegisterJobWithCron();
        }

        public static DateTimeOffset RegisterJobWithCron<T>(string cron)
            where T : IJob
        {
            var job = JobBuilder.Create<T>()
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithCronSchedule(cron)
                .Build();

            return QuartzProvider.Schedule.ScheduleJob(job, trigger).GetAwaiter().GetResult();
        }

        public static DateTimeOffset RegisterJob<T>(Func<TriggerBuilder, TriggerBuilder> triggerBuilder)
            where T : IJob
        {
            var job = JobBuilder.Create<T>()
                .Build();

            var trigger = triggerBuilder.Invoke(TriggerBuilder.Create())
                .Build();

            return QuartzProvider.Schedule.ScheduleJob(job, trigger).GetAwaiter().GetResult();
        }
    }
}
