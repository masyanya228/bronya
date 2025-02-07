using Bronya.Entities;

using Buratino.Maps.NHibMaps;

public class AccountNHMap : NHSubclassClassMap<Account>
{
    public AccountNHMap()
    {
        Map(x => x.LastName);
        Map(x => x.TGChatId);
        Map(x => x.TGTag);
        Map(x => x.IsPhoneRequested);
        Map(x => x.Phone);
        Map(x => x.CardNumber);
        Map(x => x.SelectedTime);
        Map(x => x.SelectedPlaces);
        Map(x => x.Waiting);
        Map(x => x.GetAccountsPage);

        References(x => x.SelectedTable)
            .Not.LazyLoad();
        References(x => x.SelectedAccount)
            .Not.LazyLoad();

        HasManyToMany(x => x.Roles)
            .Access.Property()
            .AsList()
            .Cascade.All()
            .Not.LazyLoad()
            .Table("RoleRefAccount")
            .FetchType.Join()
            .ChildKeyColumns.Add("id", 
                mapping => mapping.Name("role_id")
                    .SqlType("uuid")
                    .Not.Nullable())
            .ParentKeyColumns.Add("id", 
                mapping => mapping.Name("account_id")
                    .SqlType("uuid")
                    .Not.Nullable());
    }
}