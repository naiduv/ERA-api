namespace EmergencyRoadsideAssistance.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public bool HasReservation { get; set; }
        public Customer(int id)
        {
            Id = id;
        }
    }
}