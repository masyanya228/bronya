using Buratino.Entities.Abstractions;
using System.Linq.Expressions;

namespace Buratino.Maps.MapStructure
{
    public interface IMap<T> where T : IEntityBase
    {
        void Reference<Tprop>(Expression<Func<T, Tprop>> expression, string tableName = null);
    }
}