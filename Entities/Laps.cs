using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Laps
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

        public Laps()
        {
            
        }

        public Laps(DateTime dateStart, int driverNumber, double durationSector1, double durationSector2, double durationSector3, int i1Speed, int i2Speed, bool isPitOutLap, double lapDuration, int lapNumber, int meetingKey, List<int> segmentsSector1, List<int> segmentsSector2, List<int> segmentsSector3, int sessionKey, int stSpeed)
        {
            DateStart = dateStart;
            DriverNumber = driverNumber;
            DurationSector1 = durationSector1;
            DurationSector2 = durationSector2;
            DurationSector3 = durationSector3;
            I1Speed = i1Speed;
            I2Speed = i2Speed;
            IsPitOutLap = isPitOutLap;
            LapDuration = lapDuration;
            LapNumber = lapNumber;
            MeetingKey = meetingKey;
            SegmentsSector1 = segmentsSector1;
            SegmentsSector2 = segmentsSector2;
            SegmentsSector3 = segmentsSector3;
            SessionKey = sessionKey;
            StSpeed = stSpeed;
        }
    }
}
