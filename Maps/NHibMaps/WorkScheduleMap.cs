﻿using Bronya.Entities;
using Bronya.Maps.NHibMaps;

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
        Map(x => x.AutoCancelBook);
        Map(x => x.NotificationBeforeBookEnd);
        Map(x => x.IsDayOff);
        Map(x => x.DayOfWeeks);
    }
}