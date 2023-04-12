using NpgsqlTypes;

namespace EmergencyRoadsideAssistance.Models
{
    public class Geolocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Geolocation() { }

        public Geolocation(NpgsqlPoint point)
        {
            Longitude = Convert.ToDouble(point.X);
            Latitude = Convert.ToDouble(point.Y);
        }
    }
}