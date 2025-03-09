using Bronya.Entities;
using Bronya.Maps.NHibMaps;

public class AliceDialogMap : NHSubclassClassMap<AliceDialog>
{
    public AliceDialogMap()
    {
        Map(x => x.SessionId);
        Map(x => x.UserId);
        Map(x => x.State);
        Map(x => x.SeatAmount);
        Map(x => x.Time);

        References(x => x.Table);
    }
}