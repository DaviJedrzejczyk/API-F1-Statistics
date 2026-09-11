namespace Entities.Class
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
    }
}
