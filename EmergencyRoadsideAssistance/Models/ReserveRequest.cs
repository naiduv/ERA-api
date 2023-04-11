namespace EmergencyRoadsideAssistance.Models
{
    public class ReserveRequest
    {
        public int CustomerId { get; set; }
        public Geolocation Location { get; set; }
    }
}