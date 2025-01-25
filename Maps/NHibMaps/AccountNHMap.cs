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

        References(x => x.SelectedTable)
            .Not.LazyLoad();
    }
}