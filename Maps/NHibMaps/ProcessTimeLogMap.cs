using Bronya.Entities;
using Buratino.Maps.NHibMaps;

public class ProcessTimeLogMap : NHSubclassClassMap<ProcessTimeLog>
{
    public ProcessTimeLogMap()
    {
        Map(x => x.Milliseconds);
    }
}