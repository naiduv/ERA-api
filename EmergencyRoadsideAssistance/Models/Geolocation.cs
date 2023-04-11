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
            Latitude = Convert.ToDouble(point.X);
            Longitude = Convert.ToDouble(point.Y);
        }
    }
}