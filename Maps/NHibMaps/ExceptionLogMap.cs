using Bronya.Entities;
using Bronya.Maps.NHibMaps;

public class ExceptionLogMap : NHSubclassClassMap<ExceptionLog>
{
    public ExceptionLogMap()
    {
        Map(x => x.Message)
            .Length(500);
        Map(x => x.StackTrace)
            .Length(2000);
    }
}