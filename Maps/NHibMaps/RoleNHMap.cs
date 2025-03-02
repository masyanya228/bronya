using Bronya.Entities;
using Bronya.Maps.NHibMaps;

public class RoleNHMap : NHSubclassClassMap<Role>
{
    public RoleNHMap()
    {
        Map(x => x.Title);
    }
}