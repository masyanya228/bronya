namespace Bronya.Entities.Abstractions
{
    public interface IEntityBase
    {
        Guid Id { get; set; }
        DateTime TimeStamp { get; set; }
        Account Account { get; set; }
    }
}