using Bronya.Entities.Abstractions;
using Bronya.Enums;

namespace Bronya.Entities
{
    public class AliceDialog : PersistentEntity
    {
        public virtual string SessionId { get; set; }

        public virtual string UserId { get; set; }

        public virtual AliceDialogState State { get; set; }

        public virtual int SeatAmount { get; set; }

        public virtual DateTime Time { get; set; }

        public virtual Table Table { get; set; }
    }
}
