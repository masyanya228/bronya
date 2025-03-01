using Bronya.Entities;
using Bronya.Entities.Abstractions;

using Buratino.Models.DomainService;
using Buratino.Models.DomainService.DomainStructure;
using Buratino.Repositories.RepositoryStructure;
using Buratino.Xtensions;

namespace Buratino.DI
{
    public class Container
    {
        internal static IServiceProvider _serviceProvider = null;

        public static bool IsReady { get => _serviceProvider is not null; }

        /// <summary>
        /// Configure ServiceActivator with full serviceProvider
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static void Configure(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Create a scope where use this ServiceActivator
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IServiceScope GetScope(IServiceProvider serviceProvider = null)
        {
            var provider = serviceProvider ?? _serviceProvider;
            return provider?
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
        }

        public static IServiceProvider GetServiceProvider()
        {
            return _serviceProvider;
        }

        public static T Get<T>(string key = null)
        {
            if (key is null)
                return _serviceProvider.GetService<T>();
            else
                return _serviceProvider.GetKeyedService<T>(key);
        }

        public static object Get(Type type, string key = null)
        {
            if (key is null)
                return _serviceProvider.GetService(type);
            else
                return _serviceProvider.GetKeyedServices(type, key).First();
        }

        public static IRepository<T> GetRepository<T>(string key = null) where T : IEntityBase
        {
            return Get<IRepository<T>>(key);
        }

        public static IDomainService<T> GetDomainService<T>(Account account, string key = null) where T : IEntityBase
        {
            var ds = Get<IDomainService<T>>(key);
            ds.Account = account;
            return ds;
        }

        public static object GetDomainService(Type type, Account account, string key = null)
        {
            var genericType = typeof(IDomainService<>).MakeGenericType(type);
            var ds = Get(genericType, key);
            ds.SetProp("Account", account);
            return ds;
        }

        public static ObjectDomainService GetObjectDomainService(Type type, Account account)
        {
            return new ObjectDomainService(GetDomainService(type, account));
        }
    }
}
