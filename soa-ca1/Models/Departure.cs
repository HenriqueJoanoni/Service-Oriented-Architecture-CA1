namespace soa_ca1.Models
{
    public class Departure : IComparable<Departure>
    {
        public string TrainCode { get; set; } = string.Empty;
        public string TrainType { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LastLocation { get; set; } = string.Empty;
        public string ScheduledDeparture { get; set; } = string.Empty;
        public string ExpectedDeparture { get; set; } = string.Empty;
        public int DueIn { get; set; }
        public int Late { get; set; }

        public bool IsLate => Late > 0;

        // "No Information" means the train hasn't started its journey yet, so it can't be on time or late
        public string StatusLabel
        {
            get
            {
                if (IsLate) return $"+{Late} min";
                if (Status == "En Route") return "On time";
                return "Scheduled";
            }
        }

        public int CompareTo(Departure? other)
        {
            if (other is null) return 1;

            return DueIn.CompareTo(other.DueIn);
        }
    }
}
