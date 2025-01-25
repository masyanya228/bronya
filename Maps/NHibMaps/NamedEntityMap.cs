using Buratino.Entities.Abstractions;
using Buratino.Maps.NHibMaps;

public class NamedEntityMap : NHSubclassClassMap<NamedEntity>
{
    public NamedEntityMap()
    {
        Abstract();
        Map(x => x.Name);
    }
}