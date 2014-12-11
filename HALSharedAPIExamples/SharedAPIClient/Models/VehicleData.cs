using System;

namespace SharedAPIClient.Models
{
    [Serializable]
    public class VehicleData
    {
        public string Model { get; set; }
        public string Vin { get; set; }
        public string Make { get; set; }
        public int Year { get; set; }
        public string Nickname { get; set; }
        public string Description { get; set; }
    }
}