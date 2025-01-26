using Bronya.Entities;

namespace Bronya.Dtos
{
    public class SmenaDto
    {
        public DateTime SmenaStart { get; set; }

        public DateTime SmenaEnd { get; set; }
        
        public DateTime MinimumTimeToBook { get; set; }

        public WorkSchedule Schedule { get; set; }
    }
}
