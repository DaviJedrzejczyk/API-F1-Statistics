namespace Entities
{
    public class CarData
    {
        public int Brake { get; set; }
        public DateTime Date { get; set; }
        public int DriveNumber { get; set; }
        public int Drs { get; set; }
        public int MeetingKey { get; set; }
        public int Gear { get; set; }
        public int Rpm { get; set; }
        public int SessionKey { get; set; }
        public int Speed { get; set; }
        public int Throttle { get; set; }

        public CarData()
        {
            
        }

        public CarData(int brake, DateTime date, int driveNumber, int drs, int meetingKey, int gear, int rpm, int sessionKey, int speed, int throttle)
        {
            Brake = brake;
            Date = date;
            DriveNumber = driveNumber;
            Drs = drs;
            MeetingKey = meetingKey;
            Gear = gear;
            Rpm = rpm;
            SessionKey = sessionKey;
            Speed = speed;
            Throttle = throttle;
        }
    }
}
