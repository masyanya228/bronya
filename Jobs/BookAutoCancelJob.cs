using Bronya.Services;
using Bronya.Jobs.Structures;

namespace BannerWebIS.Jobs
{
    public class BookAutoCancelJob : JobCronBase
    {
        public override string CroneTime { get; set; } = $"0 0/5 * ? * * *";

        public override void Execute()
        {
            new BookAutoCancelService().CancelBooks();
        }
    }
}