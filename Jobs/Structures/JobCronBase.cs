namespace Bronya.Jobs.Structures
{
    public abstract class JobCronBase : JobBase
    {
        public virtual string CroneTime { get; set; }
    }
}
