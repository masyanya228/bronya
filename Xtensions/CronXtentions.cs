using Quartz;
using Bronya.Jobs.Structures;
using Bronya.DI;

namespace Buratino.Xtensions
{
    public static class CronXtentions
    {
        public static DateTimeOffset RegisterJobWithCron(this JobCronBase job)
        {
            var jobDetail = JobBuilder.Create(job.GetType())
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithCronSchedule(job.CroneTime)
                .Build();
            

            return Container.Get<IQuartzProvider>().Schedule.ScheduleJob(jobDetail, trigger).GetAwaiter().GetResult();
        }
    }
}
