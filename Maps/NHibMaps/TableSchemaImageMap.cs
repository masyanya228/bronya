using Bronya.Entities;
using Buratino.Maps.NHibMaps;

public class TableSchemaImageMap : NHSubclassClassMap<TableSchemaImage>
{
    public TableSchemaImageMap()
    {
        Map(x => x.ImageId);
    }
}