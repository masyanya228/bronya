using BannerWebIS.Jobs;
using BannerWebIS.Models.Xtensions;
using BannerWebIS.Services;

using PowerManager;

using Quartz;

using System;

namespace BannerWebIS.Registrators
{
    public class JobRegistrator
    {
        public static IQuartzProvider QuartzProvider { get; set; } = Container.Resolve<IQuartzProvider>();

        public static void RegisterJobs()
        {
            new BackupJob().RegisterJobWithCron();
            RegisterJobWithCron<ManagerDailyReportJob>($"0 0 16 * * ?");
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
