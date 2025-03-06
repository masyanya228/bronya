using Bronya.Helpers;
using Bronya.Xtensions;

using System.Runtime.Caching;

namespace Bronya.Caching.Structure
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Сущность, которая хранится в кэше</typeparam>
    public abstract class InMemoryCacheService<T> : ICacheService<T> where T : class
    {
        protected ObjectCache Cache { get; set; }
        protected CacheItemPolicyProxy CacheItemPolicy { get; }
        protected TimeSpan? TimeExpiration { get; set; }

        protected InMemoryCacheService()
        {
            Cache = new MemoryCache(GetType().Name);
            CacheItemPolicy = new CacheItemPolicyProxy();
            CacheItemPolicy.RemovedCallback += OnRemoved;
        }

        public virtual T Get(Func<T> func, string key, TimeSpan? timeExpiration = default)
        {
            var cacheItem = Cache.Get(key);
            if (cacheItem == default)
            {
                if (func == default)
                    return null;
                timeExpiration ??= TimeExpiration ?? TimeSpan.FromMinutes(1);
                cacheItem = func();
                if (cacheItem == default)
                    return null;
                Cache.Add(key, cacheItem, CacheItemPolicy.SetTimeExpiration(timeExpiration.Value));
                ConsoleXtensions.ColoredPrint($"{GetType().Name}: {key}", ConsoleColor.Red);
            }
            else
            {
                ConsoleXtensions.ColoredPrint($"{GetType().Name}: {key}", ConsoleColor.Cyan);
            }
            return (T)cacheItem;
        }

        public virtual T Set(string key, T value, TimeSpan? timeExpiration = null)
        {
            Cache.Set(key, value, CacheItemPolicy.SetTimeExpiration(timeExpiration ?? TimeExpiration ?? TimeSpan.FromMinutes(1)));
            ConsoleXtensions.ColoredPrint($"{GetType().Name}: {key}", ConsoleColor.Red);
            return value;
        }

        public abstract object Remove(T entity);

        public virtual void OnRemoved(CacheEntryRemovedArguments removedArguments)
        {
            ConsoleXtensions.ColoredPrint($"{GetType().Name}: {removedArguments.CacheItem.Key}", ConsoleColor.DarkRed);
        }

        public virtual string GetKey(T entity)
        {
            return entity.ToString();
        }
    }
}
