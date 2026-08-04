using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
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
        public double StopDuration { get; set; }

        public Pit()
        {
            
        }

        public Pit(DateTime date, int driverNumber, double laneDuration, int lapNumber, int meetingKey, double pitDuration, int sessionKey, double stopDuration)
        {
            Date = date;
            DriverNumber = driverNumber;
            LaneDuration = laneDuration;
            LapNumber = lapNumber;
            MeetingKey = meetingKey;
            PitDuration = pitDuration;
            SessionKey = sessionKey;
            StopDuration = stopDuration;
        }
    }
}
