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
        private Mock<IUnityOfWork> _unityOfWorkMock = null!;
        private MeetingService    _service          = null!;
        private List<Meeting>     _meetings         = null!;
        

        [SetUp]
        public void Setup()
        {
            _unityOfWorkMock = new Mock<IUnityOfWork>();
            _service         = new MeetingService(_unityOfWorkMock.Object);
            _meetings        = CreateMeetingsToInsert();
        }

        [Test]
        public async Task ShouldBeInsertAllMeetingOfCurrentYear()
        {
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.InsertTracksOfCurrentYear(_meetings)).ReturnsAsync(new Response() { HasSuccess = true});

            var service = await _service.InsertTracksOfCurrentYear(_meetings);

            Assert.That(service.Message, Is.EqualTo("All meetings inserted successfully."));
        }

       [Test]
       public async Task ShouldBeInsertOnlyNewMeetingOfCurrentYear()
        {
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(1)).ReturnsAsync(new SingleResponse<Meeting>() { Item = new Meeting() });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(2)).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(3)).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.InsertTracksOfCurrentYear(It.Is<List<Meeting>>(m => m.Count == 2))).ReturnsAsync(new Response() { HasSuccess = true });

            var service = await _service.InsertTracksOfCurrentYear(_meetings);
            
            Assert.That(service.Message, Is.EqualTo("All meetings inserted successfully."));
        }

        [Test]
        public async Task ShouldBeReturnExceptionIfMeetingKeyIsNotFound()
        {
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(1)).ReturnsAsync(new SingleResponse<Meeting>() { Item = new Meeting() });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(2)).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(3)).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.InsertTracksOfCurrentYear(It.Is<List<Meeting>>(m => m.Count == 2))).ThrowsAsync(new Exception("No meeting found with the specified key."));
            
            Response ex = await _service.InsertTracksOfCurrentYear(_meetings);

            Assert.That(ex.Message, Is.EqualTo("No meeting found with the specified key."));
        }

        [Test]
        public async Task ShouldBeReturnExceptionIfInsertMeetingFailed()
        {
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.InsertTracksOfCurrentYear(_meetings)).ThrowsAsync(new Exception("Failed to insert meetings."));
            
            Response ex = await _service.InsertTracksOfCurrentYear(_meetings);
            
            Assert.That(ex.Message, Is.EqualTo("Failed to insert meetings."));
        }

        [Test]
        public async Task ShouldBeNotInsertMeeting()
        {
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = new Meeting { CircuitKey = 1 } });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = new Meeting { CircuitKey = 2 } });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = new Meeting { CircuitKey = 3 } });
         
            var service = await _service.InsertTracksOfCurrentYear(_meetings);
            Assert.That(service.Message, Is.EqualTo("No new meetings to insert."));
        }

        [Test]
        public async Task ShouldBeReturnFailureIfMeetingKeyIsLessThanZero()
        {            
            var service = await _service.GetMeetingByKey(-1);
         
            Assert.That(service.Message, Is.EqualTo("Meeting key must be greater than 0"));
        }

        [Test]
        public async Task ShouldBeReturnNullIfMeetingKeyNotFound()
        {
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(999)).ReturnsAsync(new SingleResponse<Meeting>());
            var service = await _service.GetMeetingByKey(999);
            Assert.Multiple(() =>
            {
                Assert.That(service.Item, Is.Null);
                Assert.That(service.Message, Is.EqualTo("No meeting found with the specified key."));
            });
        }


        [Test]
        public async Task ShouldBeReturnExceptionWhenMeetingDaoIsNull()
        {
            var service = await _service.GetMeetingByKey(999);

            Assert.Multiple(() =>
            {
                Assert.That(service.Item, Is.Null);
                Assert.That(service.Message, Is.EqualTo("Object reference not set to an instance of an object."));
            });
        }

        [Test]
        public async Task ShouldBeReturnMessageWhenInsertFails()
        {
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.InsertTracksOfCurrentYear(_meetings)).ReturnsAsync(new Response() { HasSuccess = false, Message = "Failed to insert meetings." });
            
            var service = await _service.InsertTracksOfCurrentYear(_meetings);
            
            Assert.That(service.Message, Is.EqualTo("Failed to insert meetings."));
        }

        [Test]
        public async Task ShouldBeReturnMessageWhenSaveFails()
        {
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.InsertTracksOfCurrentYear(_meetings)).ReturnsAsync(new Response() { HasSuccess = true });
            _unityOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(new Response() { HasSuccess = false, Message = "Failed to save meetings." });

            var service = await _service.InsertTracksOfCurrentYear(_meetings);

            Assert.That(service.Message, Is.EqualTo("Failed to save meetings."));
        }

        private List<Meeting> CreateMeetingsToInsert() => [
                new Meeting { MeetingKey = 1, MeetingName = "Meeting 1", Year = DateTime.Now.Year },
                new Meeting { MeetingKey = 2, MeetingName = "Meeting 2", Year = DateTime.Now.Year },
                new Meeting { MeetingKey = 3, MeetingName = "Meeting 3", Year = DateTime.Now.Year }
                ];
    }
}
