namespace WebApi.ViewModels
{
    public class MeetingViewModel
    {
        public int CircuitKey { get; set; }

        public string CircuitInfoUrl { get; set; } = string.Empty;

        public string CircuitImage { get; set; } = string.Empty;

        public string CircuitShortName { get; set; } = string.Empty;

        public string CircuitType { get; set; } = string.Empty;

        public string CountryCode { get; set; } = string.Empty;

        public string CountryFlag { get; set; } = string.Empty;

        public int CountryKey { get; set; }

        public string CountryName { get; set; } = string.Empty;

        public DateTimeOffset DateEnd { get; set; }

        public DateTimeOffset DateStart { get; set; }

        public TimeSpan GmtOffset { get; set; }

        public bool IsCancelled { get; set; }

        public string Location { get; set; } = string.Empty;

        public int MeetingKey { get; set; }

        public string MeetingName { get; set; } = string.Empty;

        public string MeetingOfficialName { get; set; } = string.Empty;

        public int Year { get; set; }
    }
}
