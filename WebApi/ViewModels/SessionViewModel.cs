namespace WebApi.ViewModels
{
    public class SessionViewModel
    {
        public int SessionKey { get; set; }
        public int CircuitKey { get; set; }
        public string CircuitShortName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public int CountryKey { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public DateTimeOffset DateStart { get; set; }
        public DateTimeOffset DateEnd { get; set; }
        public TimeSpan GmtOffset { get; set; }
        public bool IsCancelled { get; set; }
        public string Location { get; set; } = string.Empty;
        public int MeetingKey { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public string SessionType { get; set; } = string.Empty;
        public int Year { get; set; }
    }
}

