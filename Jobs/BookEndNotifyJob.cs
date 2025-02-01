using Bronya.Services;
using Bronya.Jobs.Structures;

namespace BannerWebIS.Jobs
{
    public class BookEndNotifyJob : JobCronBase
    {
        public override string CroneTime { get; set; } = $"0 0/1 * ? * * *";

        public override void Execute()
        {
            new BookEndNotifyService().Notify();
        }
    }
}