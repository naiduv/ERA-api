namespace EmergencyRoadsideAssistance.Models
{
    public class UpdateAssistantRequest
    {
        public int AssistantId { get; set; }
        public Geolocation Location { get; set; }
    }
}