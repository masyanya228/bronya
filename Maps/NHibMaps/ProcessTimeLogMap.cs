using Bronya.Entities;
using Bronya.Maps.NHibMaps;

public class ProcessTimeLogMap : NHSubclassClassMap<ProcessTimeLog>
{
    public ProcessTimeLogMap()
    {
        Map(x => x.Milliseconds);
    }
}