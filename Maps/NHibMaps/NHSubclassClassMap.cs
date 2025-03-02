using FluentNHibernate.Mapping;

namespace Bronya.Maps.NHibMaps
{
    public abstract class NHSubclassClassMap<T> : SubclassMap<T>, INHMap
    {
        public NHSubclassClassMap()
        {
            Table($"{typeof(T).Name}s");
        }
    }
}
