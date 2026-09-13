#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Dao.Impl;
using Dao;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shared.Responses;

namespace UnitTests.Dao
{
    [TestFixture]
    public class LapDaoTests
    {
        private ApiF1DB CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(dbName).Options;
            return new ApiF1DB(options);
        }

        [Test]
        public void LapDao_Constructor_WithValidDb_DoesNotThrow()
        {
            // Arrange
            using var db = CreateInMemoryContext(Guid.NewGuid().ToString());

            // Act
            var dao = new LapDao(db);

            // Assert
            Assert.IsNotNull(dao);
        }

        [Test]
        public async Task GetFastLapSessionBySessionKey_SessionHasFastLap_ReturnsSuccessWithLap()
        {
            // Arrange
            using var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            var lap = new Lap { SessionKey = 1, IsFastLap = true };
            await db.Laps.AddAsync(lap);
            await db.SaveChangesAsync();

            var dao = new LapDao(db);

            // Act
            var response = await dao.GetFastLapSessionBySessionKey(1);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.HasSuccess);
            Assert.IsNotNull(response.Item);
            Assert.That(response.Item.SessionKey, Is.EqualTo(1));
            Assert.That(response.Item.IsFastLap, Is.True);
        }

        [Test]
        public async Task GetFastLapSessionBySessionKey_DbThrows_ReturnsFailure()
        {
            // Arrange
            using var db = CreateInMemoryContext(Guid.NewGuid().ToString());

            var mockSet = new Mock<DbSet<Lap>>();
            // Make the query provider throw when accessed to simulate DB failure
            mockSet.As<IQueryable<Lap>>().Setup(m => m.Provider).Throws(new Exception("boom"));

            db.Laps = mockSet.Object;

            var dao = new LapDao(db);

            // Act
            var response = await dao.GetFastLapSessionBySessionKey(1);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.HasSuccess);
            Assert.IsNotNull(response.Exception);
            Assert.That(response.Exception.Message, Is.EqualTo("boom"));
        }

        [Test]
        public async Task SaveLap_AddsLapAndReturnsSuccess()
        {
            // Arrange
            using var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            var dao = new LapDao(db);

            var lap = new Lap { SessionKey = 5, IsFastLap = false };

            // Act
            var response = await dao.SaveLap(lap);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.HasSuccess);

            // Persist and verify it was added to the context
            await db.SaveChangesAsync();
            var count = await db.Laps.CountAsync(l => l.SessionKey == 5);
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task SaveLap_DbThrows_ReturnsFailure()
        {
            // Arrange
            using var db = CreateInMemoryContext(Guid.NewGuid().ToString());

            var mockSet = new Mock<DbSet<Lap>>();
            // Setup AddAsync to throw to simulate DB failure
            mockSet.Setup(m => m.AddAsync(It.IsAny<Lap>(), It.IsAny<CancellationToken>())).Throws(new Exception("boom"));

            db.Laps = mockSet.Object;

            var dao = new LapDao(db);
            var lap = new Lap { SessionKey = 7 };

            // Act
            var response = await dao.SaveLap(lap);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.HasSuccess);
            Assert.IsNotNull(response.Exception);
            Assert.That(response.Exception.Message, Is.EqualTo("boom"));
        }
    }
}
