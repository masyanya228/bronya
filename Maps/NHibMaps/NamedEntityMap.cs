using Bronya.Entities.Abstractions;
using Bronya.Maps.NHibMaps;

public class NamedEntityMap : NHSubclassClassMap<NamedEntity>
{
    public NamedEntityMap()
    {
        Abstract();
        Map(x => x.Name);
    }
}