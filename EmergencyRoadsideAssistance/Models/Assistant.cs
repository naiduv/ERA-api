using NpgsqlTypes;
using System.Text.Json.Serialization;

namespace EmergencyRoadsideAssistance.Models
{
    public class Assistant : IComparable<Assistant>
    {
        public int Id { get; set; }
        public bool IsReserved { get; set; }
        public NpgsqlPoint Location { get; set; }
        public double Distance { get; set; }
        public Assistant() { }

        public Assistant(int id)
        {
            Id = id;
        }

        public int CompareTo(Assistant? that)
        {
            if (this.Distance < that.Distance) return -1;
            if (this.Distance == that.Distance) return 0;
            return 1;

        }
    }
}