using Bronya.Entities;

using Buratino.Maps.NHibMaps;

public class RoleAccountLinkNHMap : NHSubclassClassMap<RoleAccountLink>
{
    public RoleAccountLinkNHMap()
    {
        References(item => item.Role)
            .Not.LazyLoad();
    }
}