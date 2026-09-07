using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class TeamChampionship
    {
        public int MeetingKey { get; set; }
        public int PointsCurrent { get; set; }
        public int PointsStart { get; set; }
        public int PositionCurrent { get; set; }
        public int PositionStart { get; set; }
        public int SessionKey { get; set; }
        public string TeamName { get; set; } = string.Empty;

        public TeamChampionship()
        {
            
        }

        public TeamChampionship(int meetingKey, int pointsCurrent, int pointsStart, int positionCurrent, int positionStart, int sessionKey, string teamName)
        {
            MeetingKey = meetingKey;
            PointsCurrent = pointsCurrent;
            PointsStart = pointsStart;
            PositionCurrent = positionCurrent;
            PositionStart = positionStart;
            SessionKey = sessionKey;
            TeamName = teamName;
        }
    }
}
