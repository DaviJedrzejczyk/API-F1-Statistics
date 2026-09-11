namespace Entities.Class
{
    public class Stint
    {
        public string Compound { get; set; } = string.Empty;
        public int DriverNumber { get; set; }
        public int? LapEnd { get; set; }
        public int? LapStart { get; set; }
        public int MeetingKey { get; set; }
        public int SessionKey { get; set; }
        public int StintNumber { get; set; }
        public int TyreAgeAtStart { get; set; }
    }
}
