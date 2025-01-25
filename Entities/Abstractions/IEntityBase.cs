
namespace Buratino.Entities.Abstractions
{
    public interface IEntityBase
    {
        Guid Id { get; set; }
        DateTime TimeStamp { get; set; }
    }
}