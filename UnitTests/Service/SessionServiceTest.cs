using Dao.Interface;
using Entities;
using ExternalApi.Interfaces;
using Moq;
using Services.Impl;
using Services.Interfaces;
using Shared.Responses;
using System.Collections;

namespace UnitTests.Service
{
    [TestFixture]
    public class SessionServiceTest
    {
        private Mock<IUnityOfWork> _unityOfWorkMock = null!;
        private Mock<ISessionClient> _sessionClient = null!;
        private SessionService _service = null!;
        
        [SetUp]
        public void Setup()
        {
            _unityOfWorkMock = new Mock<IUnityOfWork>();
            _sessionClient = new Mock<ISessionClient>();
            _service = new SessionService(_unityOfWorkMock.Object, _sessionClient.Object);
        }

        [Test]
        public async Task ShouldBeReturnSessionByMeetingKeySessionKey()
        {
            int meetingKey = 1;
            int sessionKey = 1;
            
            _unityOfWorkMock.Setup(x => x.SessionDao.GetSessionByMeetingKeySessionKey(meetingKey, sessionKey)).ReturnsAsync(MakeSingleResponse(true, new Session()));

            var response = await _service.GetSessionByMeetingKeySessionKey(meetingKey, sessionKey);

            Assert.That(response.Item, Is.Not.Null);
        }

        [Test]
        public async Task ShouldBeReturnExcpetionWhenSessionNotFound()
        {
            int meetingKey = 1;
            int sessionKey = 1;

            _unityOfWorkMock.Setup(x => x.SessionDao.GetSessionByMeetingKeySessionKey(meetingKey, sessionKey)).ReturnsAsync(MakeSingleResponse(false, null));
            
            SingleResponse<Session> response = await _service.GetSessionByMeetingKeySessionKey(meetingKey, sessionKey);
            
            Assert.Multiple(() =>
            {
                Assert.That(response.Item, Is.Null);
                Assert.That(response.Message, Is.EqualTo("Session not found."));
            });
        }

        [Test]
        public async Task ShouldBeThrowExceptionWhenSessionDaoThrowsException()
        {
            int meetingKey = 1;
            int sessionKey = 1;
            
            _unityOfWorkMock.Setup(x => x.SessionDao.GetSessionByMeetingKeySessionKey(meetingKey, sessionKey)).ThrowsAsync(new Exception("Database error"));

            var response = await _service.GetSessionByMeetingKeySessionKey(meetingKey, sessionKey);

            Assert.That(response.Exception, Is.Not.Null);
            Assert.That(response.Message, Is.EqualTo("Error occurred while fetching session: Database error"));
        }

        [Test]
        public async Task ShouldBeInsertSessions()
        {
            var meetingKey = 1;

            _sessionClient.Setup(x => x.GetSessionsByMeetingKey(meetingKey)).ReturnsAsync(MakeDataResponse(true));

            _unityOfWorkMock.Setup(x => x.SessionDao.InsertSessions(It.IsAny<List<Session>>())).ReturnsAsync(MakeResponseSuccess(true));

            _unityOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(MakeResponseSuccess(true));

            var response = await _service.InsertSessions(meetingKey);

            Assert.That(response.HasSuccess, Is.True);
        }

        [Test]
        public async Task ShouldBeReturnErrorIfSessionSearchFails()
        {
            var meetingKey = 1;

            _sessionClient.Setup(x => x.GetSessionsByMeetingKey(meetingKey)).ReturnsAsync(MakeDataResponse(false, "API error"));

            var response = await _service.InsertSessions(meetingKey);
            
            Assert.Multiple(() =>
            {
                Assert.That(response.HasSuccess, Is.False);
                Assert.That(response.Message, Is.EqualTo("Failed to fetch sessions: API error"));
            });
        }

        [Test]
        public async Task ShouldBeReturnErrorIfInsertSessionsFails()
        {
            var meetingKey = 1;
            
            _sessionClient.Setup(x => x.GetSessionsByMeetingKey(meetingKey)).ReturnsAsync(MakeDataResponse(true));
            
            _unityOfWorkMock.Setup(x => x.SessionDao.InsertSessions(It.IsAny<List<Session>>())).ReturnsAsync(MakeResponseSuccess(false));
            
            var response = await _service.InsertSessions(meetingKey);
            
            Assert.Multiple(() =>
            {
                Assert.That(response.HasSuccess, Is.False);
                Assert.That(response.Message, Is.EqualTo("Failed to insert sessions."));
            });
        }

        [Test]
        public async Task ShouldBeReturnErrorIfCommitFails()
        {
            var meetingKey = 1;

            _sessionClient.Setup(x => x.GetSessionsByMeetingKey(meetingKey)).ReturnsAsync(MakeDataResponse(true));

            _unityOfWorkMock.Setup(x => x.SessionDao.InsertSessions(It.IsAny<List<Session>>())).ReturnsAsync(MakeResponseSuccess(true));

            _unityOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(new Response() { HasSuccess = false });

            var response = await _service.InsertSessions(meetingKey);

            Assert.Multiple(() =>
            {
                Assert.That(response.HasSuccess, Is.False);
                Assert.That(response.Message, Is.EqualTo("Failed to commit session."));
            });
        } 
        
        [Test]
        public async Task ShouldBeReturnExceptionIfInsertSessionsThrowsException()
        {
            var meetingKey = 1;
            
            _sessionClient.Setup(x => x.GetSessionsByMeetingKey(meetingKey)).ThrowsAsync(new Exception("API error"));
            
            var response = await _service.InsertSessions(meetingKey);
         
            Assert.Multiple(() =>
            {
                Assert.That(response.HasSuccess, Is.False);
                Assert.That(response.Message, Is.EqualTo("API error"));
                Assert.That(response.Exception, Is.Not.Null);
            });
        }

        private Response MakeResponseSuccess(bool isTrue) => new() { HasSuccess = isTrue };

        private DataResponse<Session> MakeDataResponse(bool isTrue, string message = null!) => new() { Itens = [new Session()], HasSuccess = isTrue, Message = message };

        private SingleResponse<Session> MakeSingleResponse(bool isTrue, Session session = null!, string message = null!) => new() { Item = session, HasSuccess = isTrue, Message = message };

}
}
