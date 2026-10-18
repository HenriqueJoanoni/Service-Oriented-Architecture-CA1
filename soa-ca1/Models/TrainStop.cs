namespace soa_ca1.Models
{
    public class TrainStop
    {
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string LocationType { get; set; } = string.Empty;
        public int Order { get; set; }
        public string ExpectedArrival { get; set; } = string.Empty;

        // "T" = timing point: the train passes through without stopping
        public bool IsStop => LocationType != "T";
    }
}
