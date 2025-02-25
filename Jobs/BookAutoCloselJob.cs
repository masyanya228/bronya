using Bronya.Services;
using Bronya.Jobs.Structures;

namespace BannerWebIS.Jobs
{
    public class BookAutoCloselJob : JobCronBase
    {
        public override string CroneTime { get; set; } = $"0 0/30 * ? * * *";

        public override void Execute()
        {
            new BookAutoCloselService().CloseBooks();
        }
    }
}