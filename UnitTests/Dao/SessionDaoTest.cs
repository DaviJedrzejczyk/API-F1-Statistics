using Dao;
using Dao.Impl;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Dao
{
    public class SessionDaoTest
    {
        private ApiF1DB _context = null!;
        SessionDao _sessionDao = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApiF1DB>()
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .Options;

            _context = new ApiF1DB(options);

            _sessionDao = new(_context);
        }

        [Test]
        public async Task ShouldBeReturnSessionWithMeetingKeyAndSessionKey()
        {
            // Arrange
            var meetingKey = 1;
            var sessionKey = 1;

            CreateSession(meetingKey, sessionKey);

            await _context.SaveChangesAsync();

            // Act
            var result = await _sessionDao.GetSessionByMeetingKeySessionKey(meetingKey, sessionKey);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.HasSuccess, Is.True);
                Assert.That(result.Item, Is.Not.Null);
                Assert.That(result.Item!.MeetingKey, Is.EqualTo(meetingKey));
                Assert.That(result.Item.SessionKey, Is.EqualTo(sessionKey));
                Assert.That(result.Item.SessionName, Is.EqualTo("Test Session"));
            });
        }

        [Test]
        public async Task ShouldBeReturnFailureWhenSessionNotFound()
        {
            // Arrange
            var meetingKey = 1;
            var sessionKey = 1;
            // Act
            var result = await _sessionDao.GetSessionByMeetingKeySessionKey(meetingKey, sessionKey);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HasSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo($"Session with Meeting Key {meetingKey} and Session Key {sessionKey}, not found."));
        }

        [Test]
        public async Task ShouldBeReturnFailureWhenSessionNotFoundWithInvalidKeys()
        {
            // Arrange
            var meetingKey = -1;
            var sessionKey = -1;
            // Act
            var result = await _sessionDao.GetSessionByMeetingKeySessionKey(meetingKey, sessionKey);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HasSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo($"Session with Meeting Key {meetingKey} and Session Key {sessionKey}, not found."));
        }

        [Test]
        public async Task ShouldBeInsertNewSessions()
        {
            var currentYear = DateTime.Now.Year;

            List<Session> meetings = [
                new Session { MeetingKey = 1, SessionKey = 1, Year = currentYear },
                new Session { MeetingKey = 2, SessionKey = 2, Year = currentYear },
                new Session { MeetingKey = 3, SessionKey = 3, Year = currentYear }
                ];

            var response = await _sessionDao.InsertSessions(meetings);
            
            await _context.SaveChangesAsync();

            var result = await _context.Sessions.ToListAsync();

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(3));
                Assert.That(result.Any(s => s.MeetingKey == 1 && s.SessionKey == 1), Is.True);
                Assert.That(result.Any(s => s.MeetingKey == 2 && s.SessionKey == 2), Is.True);
                Assert.That(result.Any(s => s.MeetingKey == 3 && s.SessionKey == 3), Is.True);
            });


            Assert.That(response, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(response.HasSuccess, Is.True);
                Assert.That(response.Message, Is.EqualTo("The sessions were inserted successfully."));
            });
        }

        private void CreateSession(int meetingKey, int sessionKey)
        {
            _context.Sessions.Add(new Entities.Session
            {
                MeetingKey = meetingKey,
                SessionKey = sessionKey,
                SessionName = "Test Session"
            });           
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }
    }
}