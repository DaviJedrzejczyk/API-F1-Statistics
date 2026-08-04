namespace Entities
{
    public class Driver
    {
        public string BroadcastName { get; set; }
        public int DriveNumber { get; set; }
        public string FirstName { get; set; }
        public string FullName { get; set; }
        public string HeadshotUrl { get; set; }
        public string LastName { get; set; }
        public int MeetingKey { get; set; }
        public string NameAcronym { get; set; }
        public int SessionKey { get; set; }
        public string TeamColour { get; set; }
        public string TeamName { get; set; }

        public Driver()
        {
            
        }

        public Driver(string broadcastName, int driveNumber, string firstName, string fullName, string headshotUrl, string lastName, int meetingKey, string nameAcronym, int sessionKey, string teamColour, string teamName)
        {
            BroadcastName = broadcastName;
            DriveNumber = driveNumber;
            FirstName = firstName;
            FullName = fullName;
            HeadshotUrl = headshotUrl;
            LastName = lastName;
            MeetingKey = meetingKey;
            NameAcronym = nameAcronym;
            SessionKey = sessionKey;
            TeamColour = teamColour;
            TeamName = teamName;
        }
    }
}
