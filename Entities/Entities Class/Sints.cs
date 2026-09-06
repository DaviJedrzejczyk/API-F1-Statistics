using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Sints
    {
        public string Compound { get; set; } = string.Empty;
        public int DriverNumber { get; set; }
        public int LapEnd { get; set; }
        public int LapStart { get; set; }
        public int MeetingKey { get; set; }
        public int SessionKey { get; set; }
        public int StintNumber { get; set; }
        public int TyreAgeAtStart { get; set; }

        public Sints()
        {
            
        }
        public Sints(string compound, int driverNumber, int lapEnd, int lapStart, int meetingKey, int sessionKey, int stintNumber, int tyreAgeAtStart)
        {
            Compound = compound;
            DriverNumber = driverNumber;
            LapEnd = lapEnd;
            LapStart = lapStart;
            MeetingKey = meetingKey;
            SessionKey = sessionKey;
            StintNumber = stintNumber;
            TyreAgeAtStart = tyreAgeAtStart;
        }
    }
}
