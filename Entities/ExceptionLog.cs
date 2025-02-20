using Buratino.Entities.Abstractions;

namespace Bronya.Entities
{
    public class ExceptionLog : NamedEntity
    {
        public virtual string Message { get; set; }

        public virtual string StackTrace { get; set; }
    }
}