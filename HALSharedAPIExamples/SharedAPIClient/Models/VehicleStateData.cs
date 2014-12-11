using System;

namespace SharedAPIClient.Models
{
    [Serializable]
    public class VehicleStateData
    {
        public int? Temperature { get; set; }
        public double? BatteryVoltage { get; set; }
        public bool EngineOn { get; set; }
        public int? FuelLevel { get; set; }
        public string EngineLightStatus { get; set; }
        public double? Odometer { get; set; }
        public int? Speed { get; set; }
    }
}