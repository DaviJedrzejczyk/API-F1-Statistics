using Dao.Interface;
using Entities;
using Moq;
using Services.Impl;
using Shared.Responses;

namespace UnitTests.Service
{
    [TestFixture]
    public class MeetingServiceTest
    {
        private Mock<IMeetingDao> _meetingDaoMock = null!;
        private MeetingService    _service        = null!;
        private List<Meeting>     _meetings       = null!;
        

        [SetUp]
        public void Setup()
        {
            _meetingDaoMock  = new Mock<IMeetingDao>();
            _service         = new MeetingService(_meetingDaoMock.Object);
            _meetings        = CreateMeetingsToInsert();
        }

        [Test]
        public async Task ShouldBeInsertAllMeetingOfCurrentYear()
        {
            _meetingDaoMock.Setup(x => x.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _meetingDaoMock.Setup(x => x.InsertTracksOfCurrentYear(_meetings)).ReturnsAsync(new Response() { HasSuccess = true});

            var service = await _service.InsertTracksOfCurrentYear(_meetings);

            Assert.That(service.Message, Is.EqualTo("All meetings inserted successfully."));
        }

       [Test]
       public async Task ShouldBeInsertOnlyNewMeetingOfCurrentYear()
        {
            _meetingDaoMock.Setup(x => x.GetMeetingByKey(1)).ReturnsAsync(new SingleResponse<Meeting>() { Item = new Meeting() });
            _meetingDaoMock.Setup(x => x.GetMeetingByKey(2)).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _meetingDaoMock.Setup(x => x.GetMeetingByKey(3)).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _meetingDaoMock.Setup(x => x.InsertTracksOfCurrentYear(It.Is<List<Meeting>>(m => m.Count == 2))).ReturnsAsync(new Response() { HasSuccess = true });

            var service = await _service.InsertTracksOfCurrentYear(_meetings);
            
            Assert.That(service.Message, Is.EqualTo("All meetings inserted successfully."));
        }

        [Test]
        public async Task ShouldBeReturnExceptionIfMeetingKeyIsNotFound()
        {
            _meetingDaoMock.Setup(x => x.GetMeetingByKey(1)).ReturnsAsync(new SingleResponse<Meeting>() { Item = new Meeting() });
            _meetingDaoMock.Setup(x => x.GetMeetingByKey(2)).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _meetingDaoMock.Setup(x => x.GetMeetingByKey(3)).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _meetingDaoMock.Setup(x => x.InsertTracksOfCurrentYear(It.Is<List<Meeting>>(m => m.Count == 2))).ThrowsAsync(new Exception("No meeting found with the specified key."));
            
            Response ex = await _service.InsertTracksOfCurrentYear(_meetings);

            Assert.That(ex.Message, Is.EqualTo("No meeting found with the specified key."));
        }

        [Test]
        public async Task ShouldBeReturnExceptionIfInsertMeetingFailed()
        {
            _meetingDaoMock.Setup(x => x.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _meetingDaoMock.Setup(x => x.InsertTracksOfCurrentYear(_meetings)).ThrowsAsync(new Exception("Failed to insert meetings."));
            
            Response ex = await _service.InsertTracksOfCurrentYear(_meetings);
            
            Assert.That(ex.Message, Is.EqualTo("Failed to insert meetings."));
        }


        private List<Meeting> CreateMeetingsToInsert() => [
                new Meeting { MeetingKey = 1, MeetingName = "Meeting 1", Year = DateTime.Now.Year },
                new Meeting { MeetingKey = 2, MeetingName = "Meeting 2", Year = DateTime.Now.Year },
                new Meeting { MeetingKey = 3, MeetingName = "Meeting 3", Year = DateTime.Now.Year }
                ];
    }
}
