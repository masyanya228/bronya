using Bronya.Entities;
using Bronya.Maps.NHibMaps;

public class BookMap : NHSubclassClassMap<Book>
{
    public BookMap()
    {
        Map(x => x.ActualBookStartTime);
        Map(x => x.BookLength);
        Map(x => x.IsCanceled);
        Map(x => x.SeatAmount);
        Map(x => x.TableStarted);
        Map(x => x.TableAllowedStarted);
        Map(x => x.TableClosed);
        Map(x => x.Comment);
        Map(x => x.NotifiedAboutEndBook);

        References(x => x.Guest)
            .Not.LazyLoad();
        References(x => x.Table)
            .Not.LazyLoad();
    }
}