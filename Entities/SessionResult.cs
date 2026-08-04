using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class SessionResult
    {
        public bool Dnf { get; set; }
        public bool Dns { get; set; }
        public bool Dsq { get; set; }
        public int DriverNumber { get; set; }
        public double Duration { get; set; }
        public double GapToLeader { get; set; }
        public int NumberOfLaps { get; set; }
        public int MeetingKey { get; set; }
        public int Position { get; set; }
        public int SessionKey { get; set; }

        public SessionResult()
        {
            
        }

        public SessionResult(bool dnf, bool dns, bool dsq, int driverNumber, double duration, double gapToLeader, int numberOfLaps, int meetingKey, int position, int sessionKey)
        {
            Dnf = dnf;
            Dns = dns;
            Dsq = dsq;
            DriverNumber = driverNumber;
            Duration = duration;
            GapToLeader = gapToLeader;
            NumberOfLaps = numberOfLaps;
            MeetingKey = meetingKey;
            Position = position;
            SessionKey = sessionKey;
        }
    }
}
