using Bronya.Entities.Abstractions;

using Buratino.Maps.NHibMaps;

public class PersistentEntityMap : NHSubclassClassMap<PersistentEntity>
{
    public PersistentEntityMap()
    {
        Abstract();
        Map(x => x.DeletedStamp);
        Map(x => x.IsDeleted);
    }
}