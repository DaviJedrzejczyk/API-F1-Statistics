namespace Entities.Dtos
{
    public class RaceControlFilterDto
    {
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string Flag { get; set; } = string.Empty;
        public int? Sector { get; set; }
    }
}
