using Dao;
using Dao.Impl;
using Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Responses;

namespace UnitTests.Dao
{
    [TestFixture]
    public class MeetingDaoTest
    {
        private ApiF1DB _context = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApiF1DB>()
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .Options;

            _context = new ApiF1DB(options);
        }

        [Test]
        public async Task ShouldBeReturnAllMeetingsOfCurrentYear() 
        {
            //Arrange
            var currentYear = DateTime.Now.Year;

            for (int i = 1; i <= 5; i++)
            {
                _context.Meetings.Add(new Meeting
                {
                    MeetingKey = i,
                    MeetingName = $"Meeting {i}",
                    Year = currentYear
                });
            }
            await _context.SaveChangesAsync();

            MeetingDao service = new(_context);

            //Act
            DataResponse<Meeting> result = await service.GetAllTracksOfCurrentYear(currentYear);

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Itens, Has.Count.EqualTo(5));
                Assert.That(result.Itens, Is.All.Matches<Meeting>(x => x.Year == DateTime.Now.Year));
            });
        }

        [Test]
        public async Task ShouldBeInsertAllMeetingsOfCurrentYear()
        {
            //Arrange
            var currentYear = 2023;

            List<Meeting> meetings = [
                new Meeting { MeetingKey = 1, MeetingName = "Meeting 1", Year = currentYear },
                new Meeting { MeetingKey = 2, MeetingName = "Meeting 2", Year = currentYear },
                new Meeting { MeetingKey = 3, MeetingName = "Meeting 3", Year = currentYear }
                ];

            MeetingDao service = new(_context);

            //Act
            await service.InsertTracksOfCurrentYear(meetings);
            await _context.SaveChangesAsync();

            var result = await _context.Meetings.ToListAsync();  //service.GetAllTracksOfCurrentYear(currentYear);

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!, Has.Count.EqualTo(3));
                Assert.That(result!, Is.All.Matches<Meeting>(x => x.Year == currentYear));
            });
        }

        [Test]
        public async Task ShouldReturnMeetingById()
        {
            //Arrange
            _context.Meetings.Add(new Meeting
            {
               MeetingKey = 1,
               MeetingName = "Brazil GP"
            });

            await _context.SaveChangesAsync();

            var service = new MeetingDao(_context);

            // Act
            var result = await service.GetMeetingByKey(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Item.MeetingName, Is.EqualTo("Brazil GP"));
        }

        [Test]
        public async Task ShouldReturnNullIfMeetingNotFound()
        {
            // Arrange
            var service = new MeetingDao(_context);
            
            // Act
            var result = await service.GetMeetingByKey(999); 
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Item, Is.Null);
        }

        [Test]
        public async Task ShouldBeReturnExceptionWhenGetMeetingByKey()
        {
            //Arrange
            var dbContextMock = new Mock<ApiF1DB>();
            var dbSetMock     = new Mock<DbSet<Meeting>>();
            
            dbSetMock.Setup(x => x.FindAsync(-1)).Throws(new Exception("Meeting key not found"));

            MeetingDao service = new(dbContextMock.Object);
            
            //Act
            SingleResponse<Meeting> result = await service.GetMeetingByKey(-1);

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Message, Does.Contain("An error has occurred when fetching the meeting: " + result.Exception.Message));
        }

        [Test]
        public async Task ShouldBeReturnExceptionWhenFailToInsertNewMeeting()
        {
            //Arrange
            var dbContextMock = new Mock<ApiF1DB>();
            var dbSetMock = new Mock<DbSet<Meeting>>();

            dbSetMock.Setup(x => x.AddRangeAsync()).Throws(new Exception("Cannot insert meetings"));

            MeetingDao service = new(dbContextMock.Object);

            //Act
            Response result = await service.InsertTracksOfCurrentYear(new List<Meeting>());

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Message, Does.Contain("Failed to insert meetings: " + result.Exception.Message));
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }
    }
}
