using NpgsqlTypes;

namespace EmergencyRoadsideAssistance.Models
{
    public class Assistant : IComparable<Assistant>
    {
        public int Id { get; set; }
        public bool IsReserved { get; set; }
        public Geolocation Location { get; set; }
        public double Distance { get; set; }
        protected NpgsqlPoint LocPoint { set { Location = new Geolocation(value); } }

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