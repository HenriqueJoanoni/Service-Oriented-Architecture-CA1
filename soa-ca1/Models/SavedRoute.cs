namespace soa_ca1.Models
{
    public class SavedRoute : IComparable<SavedRoute>
    {
        public string Name { get; set; } = string.Empty;
        public string FromCode { get; set; } = string.Empty;
        public string ToCode { get; set; } = string.Empty;

        // Direction matters: "Home → DkIT" and "DkIT → Home" are different routes. The name is just a label.
        public override bool Equals(object? obj)
        {
            return obj is SavedRoute other
                && FromCode == other.FromCode
                && ToCode == other.ToCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FromCode, ToCode);
        }

        public static bool operator ==(SavedRoute? a, SavedRoute? b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(SavedRoute? a, SavedRoute? b)
        {
            return !Equals(a, b);
        }

        public int CompareTo(SavedRoute? other)
        {
            if (other is null)
            {
                return 1;
            }

            return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
