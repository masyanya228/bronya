using FluentNHibernate.Mapping;

namespace Buratino.Maps.NHibMaps
{
    public abstract class NHSubclassClassMap<T> : SubclassMap<T>, INHMap
    {
        public NHSubclassClassMap()
        {
            Table($"{typeof(T).Name}s");
        }
    }
}
