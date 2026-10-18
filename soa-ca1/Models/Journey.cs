namespace soa_ca1.Models
{
    public class Journey : IComparable<Journey>
    {
        public Departure Departure { get; set; } = new();
        public string ArrivalTime { get; set; } = string.Empty;

        public int CompareTo(Journey? other)
        {
            if (other is null)
            {
                return 1;
            }

            return Departure.CompareTo(other.Departure);
        }
    }
}
