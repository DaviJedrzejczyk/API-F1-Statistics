using Dao.Interface;
using Entities;
using ExternalApi.Interfaces;
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
        private Mock<IMeetingClient> _meetClientMock = null!;


        [SetUp]
        public void Setup()
        {
            _unityOfWorkMock = new Mock<IUnityOfWork>();
            _meetClientMock  = new Mock<IMeetingClient>();
            _service         = new MeetingService(_unityOfWorkMock.Object, _meetClientMock.Object);
            _meetings        = CreateMeetingsToInsert();
        }

        [Test]
        public async Task ShouldBeInsertAllMeetingOfCurrentYear()
        {
            _meetClientMock.Setup(x => x.GetMeetingsByYear(It.IsAny<int>())).ReturnsAsync(new DataResponse<Meeting>() { Itens = _meetings });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.InsertTracksOfCurrentYear(_meetings)).ReturnsAsync(new Response() { HasSuccess = true});
            _unityOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(new Response() { HasSuccess = true });

            var service = await _service.InsertTracksOfCurrentYear();

            Assert.That(service.Message, Is.EqualTo("All meetings inserted successfully."));
        }

       [Test]
       public async Task ShouldBeInsertOnlyNewMeetingOfCurrentYear()
        {
            _meetClientMock.Setup(x => x.GetMeetingsByYear(It.IsAny<int>())).ReturnsAsync(new DataResponse<Meeting>() { Itens = _meetings });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(1)).ReturnsAsync(new SingleResponse<Meeting>() { Item = new Meeting() });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(2)).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(3)).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.InsertTracksOfCurrentYear(It.Is<List<Meeting>>(m => m.Count == 2))).ReturnsAsync(new Response() { HasSuccess = true });
            _unityOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(new Response() { HasSuccess = true });

            var service = await _service.InsertTracksOfCurrentYear();
            
            Assert.That(service.Message, Is.EqualTo("All meetings inserted successfully."));
        }

        [Test]
        public async Task ShouldBeReturnExceptionIfMeetingKeyIsNotFound()
        {
            _meetClientMock.Setup(x => x.GetMeetingsByYear(It.IsAny<int>())).ReturnsAsync(new DataResponse<Meeting>() { Itens = _meetings });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(1)).ReturnsAsync(new SingleResponse<Meeting>() { Item = new Meeting() });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(2)).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(3)).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.InsertTracksOfCurrentYear(It.Is<List<Meeting>>(m => m.Count == 2))).ThrowsAsync(new Exception("No meeting found with the specified key."));
            
            Response ex = await _service.InsertTracksOfCurrentYear();

            Assert.That(ex.Message, Is.EqualTo("No meeting found with the specified key."));
        }

        [Test]
        public async Task ShouldBeReturnExceptionIfInsertMeetingFailed()
        {
            _meetClientMock.Setup(x => x.GetMeetingsByYear(It.IsAny<int>())).ReturnsAsync(new DataResponse<Meeting>() { Itens = _meetings });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.InsertTracksOfCurrentYear(_meetings)).ThrowsAsync(new Exception("Failed to insert meetings."));
            
            Response ex = await _service.InsertTracksOfCurrentYear();
            
            Assert.That(ex.Message, Is.EqualTo("Failed to insert meetings."));
        }

        [Test]
        public async Task ShouldBeNotInsertMeeting()
        {
            _meetClientMock.Setup(x => x.GetMeetingsByYear(It.IsAny<int>())).ReturnsAsync(new DataResponse<Meeting>() { Itens = _meetings });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = new Meeting { CircuitKey = 1 } });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = new Meeting { CircuitKey = 2 } });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = new Meeting { CircuitKey = 3 } });
         
            var service = await _service.InsertTracksOfCurrentYear();
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
            _meetClientMock.Setup(x => x.GetMeetingsByYear(It.IsAny<int>())).ReturnsAsync(new DataResponse<Meeting>() { Itens = _meetings });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.InsertTracksOfCurrentYear(_meetings)).ReturnsAsync(new Response() { HasSuccess = false, Message = "Failed to insert meetings." });
            
            var service = await _service.InsertTracksOfCurrentYear();
            
            Assert.That(service.Message, Is.EqualTo("Failed to insert meetings."));
        }

        [Test]
        public async Task ShouldBeReturnMessageWhenSaveFails()
        {
            _meetClientMock.Setup(x => x.GetMeetingsByYear(It.IsAny<int>())).ReturnsAsync(new DataResponse<Meeting>() { Itens = _meetings });
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetMeetingByKey(It.IsAny<int>())).ReturnsAsync(new SingleResponse<Meeting>() { Item = null });
            _unityOfWorkMock.Setup(x => x.MeetingDao.InsertTracksOfCurrentYear(_meetings)).ReturnsAsync(new Response() { HasSuccess = true });
            _unityOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(new Response() { HasSuccess = false, Message = "Failed to save meetings." });

            var service = await _service.InsertTracksOfCurrentYear();

            Assert.That(service.Message, Is.EqualTo("Failed to save meetings."));
            Assert.That(service.HasSuccess, Is.False);
        }

        [Test]
        public async Task ShouldBeReturnAllTrakcsOfCurrentYear()
        {
            _unityOfWorkMock.Setup(x => x.MeetingDao.GetAllTracksOfCurrentYear(It.IsAny<int>())).ReturnsAsync(new DataResponse<Meeting>() { Itens = _meetings, HasSuccess = true });
            
            var service = await _service.GetAllTracksOfCurrentYear(DateTime.Now.Year);
            
            Assert.That(service.Itens.Count, Is.EqualTo(3));
            Assert.That(service.HasSuccess, Is.True);
        }

        [Test]
        public async Task ShouldBeReturnErrorGetAllTracksOfCurrentYearWhenYearIsInvalid()
        {
            var service = await _service.GetAllTracksOfCurrentYear(12);

            Assert.Multiple(() =>
            {
                Assert.That(service.Message, Is.EqualTo("Invalid year format."));
                Assert.That(service.HasSuccess, Is.False);
                Assert.That(service.Itens, Is.Null);
            });
        }

        [Test]
        public async Task ShouldBeReturnErrorGetAllTracksOfCurrentYearWhenYearIsLessThan1951()
        {
            var service = await _service.GetAllTracksOfCurrentYear(1950);
         
            Assert.Multiple(() =>
            {
                Assert.That(service.Message, Is.EqualTo("Year must be between 1951 and the current year."));
                Assert.That(service.HasSuccess, Is.False);
                Assert.That(service.Itens, Is.Null);
            });
        }


        private List<Meeting> CreateMeetingsToInsert() => [
                new Meeting { MeetingKey = 1, MeetingName = "Meeting 1", Year = DateTime.Now.Year },
                new Meeting { MeetingKey = 2, MeetingName = "Meeting 2", Year = DateTime.Now.Year },
                new Meeting { MeetingKey = 3, MeetingName = "Meeting 3", Year = DateTime.Now.Year }
                ];
    }
}
