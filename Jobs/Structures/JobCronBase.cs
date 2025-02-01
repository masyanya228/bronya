namespace Bronya.Jobs.Structures
{
    public abstract class JobCronBase : JobBase
    {
        public abstract string CroneTime { get; set; }
    }
}
