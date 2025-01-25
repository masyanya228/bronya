using Bronya.Entities;

using Buratino.Maps.NHibMaps;

public class BookMap : NHSubclassClassMap<Book>
{
    public BookMap()
    {
        Map(x => x.ActualBookStartTime);
        Map(x => x.BookEndTime);
        Map(x => x.IsCanceled);
        Map(x => x.SeatAmount);
        Map(x => x.TableStarted);
        Map(x => x.TableClosed);

        References(x => x.Table)
            .Not.LazyLoad();
    }
}