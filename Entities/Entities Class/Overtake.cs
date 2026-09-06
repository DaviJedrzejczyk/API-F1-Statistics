namespace Entities
{
    public class Overtake
    {
        public int MeetingKey { get; set; }
        public int SessionKey { get; set; }
        public int OvertakingDriverNumber { get; set; }
        public int OvertakedDriverNumber { get; set; }
        public DateTime Date { get; set; }
        public int Position { get; set; }
    }
}
