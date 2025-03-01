using System.Runtime.Caching;

namespace Bronya.Helpers
{
    public class CacheItemPolicyProxy : CacheItemPolicy, ICloneable
    {
        public CacheItemPolicyProxy SetTimeExpiration(TimeSpan timeExpiration)
        {
            var clone = Clone() as CacheItemPolicyProxy;
            clone.AbsoluteExpiration = DateTime.Now.Add(timeExpiration);
            return clone;
        }

        public object Clone()
        {
            return new CacheItemPolicyProxy()
            {
                AbsoluteExpiration = this.AbsoluteExpiration,
                SlidingExpiration = this.SlidingExpiration,
                Priority = this.Priority,
                RemovedCallback = this.RemovedCallback,
                UpdateCallback = this.UpdateCallback,
            };
        }
    }
}
