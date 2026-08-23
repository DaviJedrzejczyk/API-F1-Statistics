using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class DriverChampionship
    {
        public int DriverNumber { get; set; }
        public int MeetingKey { get; set; }
        public int PointsCurrent { get; set; }
        public int PointsStart { get; set; }
        public int PositionCurrent { get; set; }
        public int PositionStart { get; set; }
        public int SessionKey { get; set; }
    }
}
