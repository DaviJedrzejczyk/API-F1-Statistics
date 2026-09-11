namespace Entities.Class
{
    public class Pit
    {
        public DateTime Date { get; set; }
        public int DriverNumber { get; set; }
        public double LaneDuration { get; set; }
        public int LapNumber { get; set; }
        public int MeetingKey { get; set; }
        public double PitDuration { get; set; }
        public int SessionKey { get; set; }
        public double? StopDuration { get; set; }
    }
}
