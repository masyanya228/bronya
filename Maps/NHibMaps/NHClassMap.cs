using FluentNHibernate.Mapping;

namespace Bronya.Maps.NHibMaps
{
    public abstract class NHClassMap<T> : ClassMap<T>, INHMap
    {
    }
}
