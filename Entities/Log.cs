using Bronya.Entities.Abstractions;

namespace Bronya.Entities
{
    public class Log : NamedEntity
    {
        public override string ToString()
        {
            return $"{Name} {TimeStamp:T}";
        }
    }
}