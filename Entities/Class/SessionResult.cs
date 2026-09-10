namespace Entities
{
    public class SessionResult
    {
        public bool Dnf { get; set; }
        public bool Dns { get; set; }
        public bool Dsq { get; set; }
        public int DriverNumber { get; set; }
        public double Duration { get; set; }
        public string GapToLeader { get; set; } = string.Empty;
        public int NumberOfLaps { get; set; }
        public double Points { get; set; }
        public int MeetingKey { get; set; }
        public int Position { get; set; }
        public int SessionKey { get; set; }
        public bool IsQualy { get; set; }
        public string Compound { get; set; } = string.Empty;
    }
}
