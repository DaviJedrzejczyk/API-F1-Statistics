namespace Entities.Class
{
    public class SessionResultQualify
    {
        public int SessionKey { get; set; }
        public int MeetingKey { get; set; }
        public int DriverNumber { get; set; }
        public string QualifyingPhase { get; set; } = string.Empty;
        public double Duration { get; set; }
        public double GapToLeader { get; set; }
    }
}
