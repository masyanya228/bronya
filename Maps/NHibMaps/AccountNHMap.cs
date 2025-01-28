using Buratino.Entities;
using Buratino.Maps.NHibMaps;

public class AccountNHMap : NHSubclassClassMap<Account>
{
    public AccountNHMap()
    {
        Map(x => x.LastName);
        Map(x => x.TGChatId);
        Map(x => x.TGTag);
        Map(x => x.SelectedTime);
        Map(x => x.SelectedPlaces);
        Map(x => x.Waiting);

        References(x => x.SelectedTable)
            .Not.LazyLoad();
    }
}