using Bronya.Entities;
using Buratino.Maps.NHibMaps;

public class ReferalMap : NHSubclassClassMap<Referal>
{
    public ReferalMap()
    {
        Map(x => x.IsPaid)
            .Default("False");

        References(x => x.Main)
            .Not.LazyLoad();
        References(x => x.NewSub)
            .Not.LazyLoad();
    }
}