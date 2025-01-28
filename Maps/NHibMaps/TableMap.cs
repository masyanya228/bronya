using Bronya.Entities;

using Buratino.Maps.NHibMaps;

public class TableMap : NHSubclassClassMap<Table>
{
    public TableMap()
    {
        Map(x => x.HasConsole);
        Map(x => x.NormalSeatAmount);
        Map(x => x.IsBookAvailable);
        Map(x => x.Number);
    }
}