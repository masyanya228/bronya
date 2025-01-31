using Buratino.Jobs.Structures;
using Buratino.DI;

namespace BannerWebIS.Jobs
{
    public class BackupJob : JobCronBase
    {
        public override string CroneTime { get; set; } = $"{Container.Get<IConfigService>().Config.BackupTime} ? * * *";

        public override void Execute()
        {
            Container.Resolve<IBackupService>().MakeBackup(null);
        }
    }
}