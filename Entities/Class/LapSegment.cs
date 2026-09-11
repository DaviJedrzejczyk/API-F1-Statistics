namespace Entities.Class
{
    public class LapSegment
    {
        public int MeetingKey { get; set; }
        public int SessionKey { get; set; }
        public int DriverNumber { get; set; }
        public int LapNumber { get; set; }
        public int Sector { get; set; }
        public int SegmentIndex { get; set; }
        public int? SegmentStatus { get; set; }
        public Lap Lap { get; set; }
    }
}
