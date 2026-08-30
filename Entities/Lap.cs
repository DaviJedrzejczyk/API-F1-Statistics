namespace Entities
{
    public class Lap
    {
        public DateTime DateStart { get; set; }
        public int DriverNumber { get; set; }
        public double DurationSector1 { get; set; }
        public double DurationSector2 { get; set; }
        public double DurationSector3 { get; set; }
        public int I1Speed { get; set; }
        public int I2Speed { get; set; }
        public bool IsPitOutLap { get; set; }
        public double LapDuration { get; set; }
        public int LapNumber { get; set; }
        public int MeetingKey { get; set; }
        public List<int> SegmentsSector1 { get; set; } = [];
        public List<int> SegmentsSector2 { get; set; } = [];
        public List<int> SegmentsSector3 { get; set; } = [];
        public int SessionKey { get; set; }
        public int StSpeed { get; set; }
    }
}
