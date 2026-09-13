using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Dao.Impl;
using Dao;
using Dao.Interface;
using Shared.Responses;

namespace UnitTests.Dao
{
    [TestFixture]
    public class UnityOfWorkTests
    {
        [Test]
        public void Constructor_WithProvidedDaos_AssignsThemToProperties()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var sessionMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            // Act
            var uow = new UnityOfWork(
                mockDb.Object,
                sessionMock.Object,
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Assert
            Assert.That(uow.SessionDao, Is.SameAs(sessionMock.Object));
            Assert.That(uow.MeetingDao, Is.SameAs(meetingMock.Object));
            Assert.That(uow.DriverDao, Is.SameAs(driverMock.Object));
        }

        [Test]
        public async Task Commit_WhenSaveChangesSucceeds_ReturnsSuccessResponse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);
            mockDb.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var anyMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                anyMock.Object,
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Act
            var response = await uow.Commit();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.HasSuccess);
            Assert.IsNull(response.Exception);
        }

        [Test]
        public async Task Commit_WhenSaveChangesThrows_ReturnsFailureResponseContainingException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);
            var ex = new InvalidOperationException("boom");
            mockDb.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(ex);

            var anyMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                anyMock.Object,
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Act
            var response = await uow.Commit();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.HasSuccess);
            Assert.IsNotNull(response.Exception);
            Assert.That(response.Message, Is.EqualTo("boom"));
            Assert.That(response.Exception, Is.SameAs(ex));
        }

        [Test]
        public void SessionDao_WhenNullCreatesInstanceAndReturnsSameInstanceOnSubsequentCalls()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                null!, // sessionDao intentionally null to trigger lazy creation
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Act
            var first = uow.SessionDao;
            var second = uow.SessionDao;

            // Assert
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.That(second, Is.SameAs(first));
            Assert.IsInstanceOf<SessionDao>(first);
        }

        [Test]
        public void MeetingDao_WhenNullCreatesInstance()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var sessionMock = new Mock<ISessionDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                sessionMock.Object,
                null!, // meetingDao intentionally null
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Act
            var meeting = uow.MeetingDao;

            // Assert
            Assert.IsNotNull(meeting);
            Assert.IsInstanceOf<MeetingDao>(meeting);
        }

        [Test]
        public void DriverDao_WhenNullCreatesInstance()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var sessionMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                sessionMock.Object,
                meetingMock.Object,
                null!, // driverDao intentionally null
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Act
            var driver = uow.DriverDao;

            // Assert
            Assert.IsNotNull(driver);
            Assert.IsInstanceOf<DriverDao>(driver);
        }

        [Test]
        public void CarDataDao_WhenNullCreatesInstanceAndReturnsSameInstanceOnSubsequentCalls()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var sessionMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                sessionMock.Object,
                meetingMock.Object,
                driverMock.Object,
                null!, // carDataDao intentionally null
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Act
            var first = uow.CarDataDao;
            var second = uow.CarDataDao;

            // Assert
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.That(second, Is.SameAs(first));
            Assert.IsInstanceOf<CarDataDao>(first);
        }

        [Test]
        public void SessionResultDao_WhenNullCreatesInstanceAndReturnsSameInstanceOnSubsequentCalls()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var sessionMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                sessionMock.Object,
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                null!, // resultDao intentionally null
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Act
            var first = uow.SessionResultDao;
            var second = uow.SessionResultDao;

            // Assert
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.That(second, Is.SameAs(first));
            Assert.IsInstanceOf<SessionResultDao>(first);
        }

        [Test]
        public void OvertakeDao_WhenNullCreatesInstanceAndReturnsSameInstanceOnSubsequentCalls()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var sessionMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                sessionMock.Object,
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                null!, // overtakeDao intentionally null
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Act
            var first = uow.OvertakeDao;
            var second = uow.OvertakeDao;

            // Assert
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.That(second, Is.SameAs(first));
            Assert.IsInstanceOf<OvertakeDao>(first);
        }

        [Test]
        public void PitDao_WhenNullCreatesInstanceAndReturnsSameInstanceOnSubsequentCalls()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var sessionMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                sessionMock.Object,
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                null!, // pitDao intentionally null
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Act
            var first = uow.PitDao;
            var second = uow.PitDao;

            // Assert
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.That(second, Is.SameAs(first));
            Assert.IsInstanceOf<PitDao>(first);
        }

        [Test]
        public void RaceControlDao_WhenNullCreatesInstanceAndReturnsSameInstanceOnSubsequentCalls()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var sessionMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                sessionMock.Object,
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                null!, // raceControlDao intentionally null
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Act
            var first = uow.RaceControlDao;
            var second = uow.RaceControlDao;

            // Assert
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.That(second, Is.SameAs(first));
            Assert.IsInstanceOf<RaceControlDao>(first);
        }


        [Test]
        public void StintDao_WhenNullCreatesInstanceAndReturnsSameInstanceOnSubsequentCalls()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var sessionMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                sessionMock.Object,
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                null!, // stintDao intentionally null
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Act
            var first = uow.StintDao;
            var second = uow.StintDao;

            // Assert
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.That(second, Is.SameAs(first));
            Assert.IsInstanceOf<StintDao>(first);
        }

        [Test]
        public void SessionResultQualifyDao_WhenNullCreatesInstanceAndReturnsSameInstanceOnSubsequentCalls()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var sessionMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                sessionMock.Object,
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                null!, // sessionResultQualifyDao intentionally null
                lapMock.Object,
                lapSegmentMock.Object);

            // Act
            var first = uow.SessionResultQualifyDao;
            var second = uow.SessionResultQualifyDao;

            // Assert
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.That(second, Is.SameAs(first));
            Assert.IsInstanceOf<SessionResultQualifyDao>(first);
        }

        [Test]
        public void LapDao_WhenNullCreatesInstanceAndReturnsSameInstanceOnSubsequentCalls()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var sessionMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                sessionMock.Object,
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                null!, // lapDao intentionally null
                lapSegmentMock.Object);

            // Act
            var first = uow.LapDao;
            var second = uow.LapDao;

            // Assert
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.That(second, Is.SameAs(first));
            Assert.IsInstanceOf<LapDao>(first);
        }

        [Test]
        public void LapSegmentDao_WhenNullCreatesInstanceAndReturnsSameInstanceOnSubsequentCalls()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var sessionMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                sessionMock.Object,
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                null!); // lapSegmentDao intentionally null

            // Act
            var first = uow.LapSegmentDao;
            var second = uow.LapSegmentDao;

            // Assert
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.That(second, Is.SameAs(first));
            Assert.IsInstanceOf<LapSegmentDao>(first);
        }

        [Test]
        public void Dispose_WhenDbProvided_CallsDbDispose()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().Options;
            var mockDb = new Mock<ApiF1DB>(options);

            var sessionMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                mockDb.Object,
                sessionMock.Object,
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Act
            uow.Dispose();

            // Assert
            mockDb.Verify(d => d.Dispose(), Times.Once);
        }

        [Test]
        public void Dispose_WhenDbIsNull_DoesNotThrow()
        {
            // Arrange
            ApiF1DB? nullDb = null;
            var sessionMock = new Mock<ISessionDao>();
            var meetingMock = new Mock<IMeetingDao>();
            var driverMock = new Mock<IDriverDao>();
            var carDataMock = new Mock<ICarDataDao>();
            var resultMock = new Mock<ISessionResultDao>();
            var overtakeMock = new Mock<IOvertakeDao>();
            var pitMock = new Mock<IPitDao>();
            var raceControlMock = new Mock<IRaceControlDao>();
            var stintMock = new Mock<IStintDao>();
            var sessionResultQualifyMock = new Mock<ISessionResultQualifyDao>();
            var lapMock = new Mock<ILapDao>();
            var lapSegmentMock = new Mock<ILapSegmentDao>();

            var uow = new UnityOfWork(
                nullDb!,
                sessionMock.Object,
                meetingMock.Object,
                driverMock.Object,
                carDataMock.Object,
                resultMock.Object,
                overtakeMock.Object,
                pitMock.Object,
                raceControlMock.Object,
                stintMock.Object,
                sessionResultQualifyMock.Object,
                lapMock.Object,
                lapSegmentMock.Object);

            // Act & Assert
            Assert.DoesNotThrow(() => uow.Dispose());
        }

    }
}
