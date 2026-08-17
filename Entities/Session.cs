using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Session
    {
        public int SessionKey { get; set; }
        public int CircuitKey { get; set; }
        public string CircuitShortName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public int CountryKey { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public DateTimeOffset DateStart { get; set; }
        public DateTimeOffset DateEnd { get; set; }
        public string GmtOffset { get; set; } = string.Empty;
        public bool IsCancelled { get; set; }
        public string Location { get; set; } = string.Empty;
        public int MeetingKey { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public string SessionType { get; set; } = string.Empty;
        public int Year { get; set; }
    }
}
