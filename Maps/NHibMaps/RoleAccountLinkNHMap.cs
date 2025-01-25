using Buratino.Entities;
using Buratino.Maps.NHibMaps;

public class RoleAccountLinkNHMap : NHSubclassClassMap<RoleAccountLink>
{
    public RoleAccountLinkNHMap()
    {
        References(item => item.Role, "RoleId")
            .Not.LazyLoad();
        
        References(item => item.Account, "AccountId")
            .Not.LazyLoad();
    }
}