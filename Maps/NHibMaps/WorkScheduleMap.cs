using Bronya.Entities;

using Buratino.Maps.NHibMaps;

public class WorkScheduleMap : NHSubclassClassMap<WorkSchedule>
{
    public WorkScheduleMap()
    {
        Map(x => x.Start);
        Map(x => x.Length);
        Map(x => x.Step);
        Map(x => x.Buffer);
        Map(x => x.MinPeriod);
        Map(x => x.StartDate);
        Map(x => x.IsOneTimeSchedule);
    }
}