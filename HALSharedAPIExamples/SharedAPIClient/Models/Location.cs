using System;

namespace SharedAPIClient.Models
{
    [Serializable]
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}