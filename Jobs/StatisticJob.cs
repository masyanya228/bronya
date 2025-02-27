using Bronya.Services;
using Bronya.Jobs.Structures;

namespace BannerWebIS.Jobs
{
    public class StatisticJob : JobCronBase
    {
        public override string CroneTime { get; set; } = $"0 0 10 ? * * *";

        public override void Execute()
        {
            new StatisticService().SendStats();
        }
    }
}