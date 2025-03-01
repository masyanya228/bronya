using Bronya.Services;
using Bronya.Jobs.Structures;

namespace BannerWebIS.Jobs
{
    public class BookAutoCloselJob : JobCronBase
    {
        public override void Execute()
        {
            new BookAutoCloselService().CloseBooks();
        }
    }
}