#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dao;
using Dao.Impl;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shared.Responses;

namespace UnitTests.Dao
{
    [TestFixture]
    public class RaceControlDaoTests
    {
        [Test]
        public async Task GetRaceControlsBySessionFlags_SessionHasMatchingFlags_ReturnsSuccessWithItems()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var db = new ApiF1DB(options);
            var now = DateTime.UtcNow;
            var item1 = new RaceControl { SessionKey = 1, Flag = "yellow", Date = now, MeetingKey = 1 };
            var item2 = new RaceControl { SessionKey = 1, Flag = "red", Date = now.AddSeconds(1), MeetingKey = 2 };
            var item3 = new RaceControl { SessionKey = 2, Flag = "yellow", Date = now.AddSeconds(2), MeetingKey = 3 };

            db.RaceControls.Add(item1);
            db.RaceControls.Add(item2);
            db.RaceControls.Add(item3);
            db.SaveChanges();

            var dao = new RaceControlDao(db);

            // Act
            var response = await dao.GetRaceControlsBySessionFlags(1, new[] { "yellow" });

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.HasSuccess);
            Assert.IsNotNull(response.Itens);
            Assert.That(response.Itens.Count, Is.EqualTo(1));
            Assert.That(response.Itens[0].Flag, Is.EqualTo("yellow"));
            Assert.That(response.Itens[0].SessionKey, Is.EqualTo(1));
        }

        [Test]
        public async Task GetRaceControlsBySessionFlags_DbThrows_ReturnsFailure()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var db = new ApiF1DB(options);

            var mockSet = new Mock<DbSet<RaceControl>>();
            // When EF queries the provider it will access Provider; make it throw to simulate DB error
            mockSet.As<IQueryable<RaceControl>>().Setup(m => m.Provider).Throws(new Exception("boom"));

            db.RaceControls = mockSet.Object;

            var dao = new RaceControlDao(db);

            // Act
            var response = await dao.GetRaceControlsBySessionFlags(1, new[] { "yellow" });

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.HasSuccess);
            Assert.That(response.Message, Is.EqualTo("Error retrieving race controls by session flags."));
            Assert.IsNotNull(response.Exception);
            Assert.That(response.Exception.Message, Is.EqualTo("boom"));
        }

        [Test]
        public async Task SaveRaceControls_AddRangeCalled_ReturnsSuccess()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var db = new ApiF1DB(options);

            var mockSet = new Mock<DbSet<RaceControl>>();
            mockSet.Setup(m => m.AddRangeAsync(It.IsAny<IEnumerable<RaceControl>>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask)
                   .Verifiable();

            db.RaceControls = mockSet.Object;

            var dao = new RaceControlDao(db);

            var toSave = new List<RaceControl>
            {
                new RaceControl { SessionKey = 1, Flag = "yellow" }
            };

            // Act
            var response = await dao.SaveRaceControls(toSave);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.HasSuccess);
            Assert.That(response.Message, Is.EqualTo("Race controls saved successfully"));
            mockSet.Verify(m => m.AddRangeAsync(It.Is<IEnumerable<RaceControl>>(r => r == toSave), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SaveRaceControls_DbThrows_ReturnsFailure()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var db = new ApiF1DB(options);

            var mockSet = new Mock<DbSet<RaceControl>>();
            mockSet.Setup(m => m.AddRangeAsync(It.IsAny<IEnumerable<RaceControl>>(), It.IsAny<CancellationToken>()))
                   .Throws(new Exception("boom"));

            db.RaceControls = mockSet.Object;

            var dao = new RaceControlDao(db);

            var toSave = new List<RaceControl>
            {
                new RaceControl { SessionKey = 1, Flag = "yellow" }
            };

            // Act
            var response = await dao.SaveRaceControls(toSave);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.HasSuccess);
            Assert.That(response.Message, Is.EqualTo("Error saving race controls"));
            Assert.IsNotNull(response.Exception);
            Assert.That(response.Exception.Message, Is.EqualTo("boom"));
        }
    }
}
