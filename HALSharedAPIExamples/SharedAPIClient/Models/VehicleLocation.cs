using System;

namespace SharedAPIClient.Models
{
    [Serializable]
    public class VehicleLocation
    {
        public Location Location { get; set; }
        public double? Heading { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
    }
}