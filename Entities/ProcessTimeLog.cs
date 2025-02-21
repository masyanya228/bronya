using Buratino.Entities.Abstractions;

namespace Bronya.Entities
{
    public class ProcessTimeLog : NamedEntity
    {
        public virtual int Milliseconds { get; set; }
    }
}