using Shared.Converters;
using System.Text.Json.Serialization;

namespace Entities.Dtos
{
    public class LapListDto
    {
        [JsonPropertyName("date_start")]
        public DateTime? DateStart { get; set; }
        
        [JsonPropertyName("driver_number")]
        public int DriverNumber { get; set; }
        
        [JsonPropertyName("duration_sector_1")]
        public double? DurationSector1 { get; set; }
        
        [JsonPropertyName("duration_sector_2")]
        public double? DurationSector2 { get; set; }
        
        [JsonPropertyName("duration_sector_3")]
        public double? DurationSector3 { get; set; }
        
        [JsonPropertyName("i1_speed")]
        public int? I1Speed { get; set; }
        
        [JsonPropertyName("i2_speed")]
        public int? I2Speed { get; set; }
        
        [JsonPropertyName("is_pit_out_lap")]
        public bool? IsPitOutLap { get; set; }
        
        [JsonPropertyName("lap_duration")]
        public double? LapDuration { get; set; }
        
        [JsonPropertyName("lap_number")]
        public int LapNumber { get; set; }
        
        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }
        
        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }
        
        [JsonPropertyName("st_speed")]
        public int? StSpeed { get; set; }

        [JsonPropertyName("segments_sector_1")]
        public List<int?>? SegmentsSector1 { get; set; }
        
        [JsonPropertyName("segments_sector_2")]
        public List<int?>? SegmentsSector2 { get; set; }
        
        [JsonPropertyName("segments_sector_3")]
        public List<int?>? SegmentsSector3 { get; set; }

        [JsonIgnore]
        public bool IsFastLap { get; set; }
    }
}
