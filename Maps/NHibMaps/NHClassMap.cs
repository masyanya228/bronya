using FluentNHibernate.Mapping;

namespace Buratino.Maps.NHibMaps
{
    public abstract class NHClassMap<T> : ClassMap<T>, INHMap
    {
    }
}
