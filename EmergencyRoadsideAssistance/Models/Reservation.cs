using NpgsqlTypes;

namespace EmergencyRoadsideAssistance.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int AssistantId { get; set; }
        public bool IsReserved { get; set; }
        public NpgsqlPoint CustomerLocation { get; set; }
        public NpgsqlPoint AssistantLocation { get; set; }
        public double Distance { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}