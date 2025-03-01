using System.Runtime.Caching;

namespace Bronya.Caching.Structure
{
    public interface ICacheService<T> where T : class
    {
        T Get(Func<T> func, string key, TimeSpan? timeExpiration = null);
        T Set(string key, T value, TimeSpan? timeExpiration = null);
        string GetKey(T entity);
        void OnRemoved(CacheEntryRemovedArguments removedArguments);
        object Remove(T entity);
    }
}