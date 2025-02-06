using Bronya.Entities;

using Buratino.Maps.NHibMaps;

public class RoleNHMap : NHSubclassClassMap<Role>
{
    public RoleNHMap()
    {
        Map(x => x.Title);
    }
}