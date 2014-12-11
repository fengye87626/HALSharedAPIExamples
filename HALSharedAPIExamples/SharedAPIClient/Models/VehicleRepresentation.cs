using System;
using SharedAPIClient.Helpers;

namespace SharedAPIClient.Models
{
    [Serializable]
    public class VehicleRepresentation : ApiRepresentation
    {
        public Guid Id { get; set; }
        public int? Year { get; set; }
        public string Make { get; set; }
        public string Vin { get; set; }
        public string Model { get; set; }
        public string FriendlyName { get; set; }
        public string Nickname { get; set; }
        public string Description { get; set; }
        public bool? IsVinEditable { get; set; }
        public bool? IsMMYEditable { get; set; }
        private Guid? ConnectedDeviceId { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
        public VehicleStateData Status { get; set; }
        public VehicleLocation VehicleLocation { get; set; }
    }
}