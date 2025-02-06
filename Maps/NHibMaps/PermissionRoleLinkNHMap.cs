using Bronya.Entities;

using Buratino.Maps.NHibMaps;

public class PermissionRoleLinkNHMap : NHSubclassClassMap<PermissionRoleLink>
{
    public PermissionRoleLinkNHMap()
    {
        Map(x => x.Permission);
        References(item => item.Role)
            .Not.LazyLoad();
    }
}