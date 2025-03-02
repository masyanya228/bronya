using Bronya.Entities;
using Bronya.Maps.NHibMaps;

public class TableSchemaImageMap : NHSubclassClassMap<TableSchemaImage>
{
    public TableSchemaImageMap()
    {
        Map(x => x.ImageId);
    }
}