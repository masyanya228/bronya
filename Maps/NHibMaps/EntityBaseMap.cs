using Buratino.Entities.Abstractions;
using Buratino.Maps.NHibMaps;

public class EntityBaseMap : NHClassMap<EntityBase>
{
    public EntityBaseMap()
    {
        UseUnionSubclassForInheritanceMapping();

        Id(item => item.Id)
            .Not.Nullable()
            .Default("gen_random_uuid()")
            .GeneratedBy.Guid();

        Map(x => x.TimeStamp)
            .Default("now()");

        References(x => x.Account);
    }
}