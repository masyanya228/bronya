using Buratino.Entities;
using Buratino.Maps.NHibMaps;

public class PermissionRoleLinkNHMap : NHSubclassClassMap<PermissionRoleLink>
{
    public PermissionRoleLinkNHMap()
    {
        Map(x => x.Permission);
        References(item => item.Role, "RoleId")
            .Not.LazyLoad();
    }
}