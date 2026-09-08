using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Dao.Impl;
using Dao;
using Shared.Responses;

namespace UnitTests.Dao
{
    [TestFixture]
    public class SessionResultQualifyDaoTests
    {
        [Test]
        public void Constructor_WithValidDb_InstanceCreated()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(databaseName: "CtorDb").Options;
            using var db = new ApiF1DB(options);

            // Act
            var dao = new SessionResultQualifyDao(db);

            // Assert
            Assert.NotNull(dao);
        }

        [Test]
        public async Task GetQualifyingResultBySessionKey_WithExistingEntries_ReturnsSuccessDataResponse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(databaseName: "GetQualifyDb").Options;
            await using var db = new ApiF1DB(options);
            var item1 = new SessionResultQualify { SessionKey = 1, MeetingKey = 10, DriverNumber = 7, QualifyingPhase = "Q1", Duration = 12.3 };
            var item2 = new SessionResultQualify { SessionKey = 2, MeetingKey = 10, DriverNumber = 8, QualifyingPhase = "Q2", Duration = 11.1 };
            db.SessionResultQualifyings.Add(item1);
            db.SessionResultQualifyings.Add(item2);
            await db.SaveChangesAsync();

            var dao = new SessionResultQualifyDao(db);

            // Act
            var response = await dao.GetQualifyingResultBySessionKey(1);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.HasSuccess);
            Assert.IsNotNull(response.Itens);
            Assert.That(response.Itens.Count, Is.EqualTo(1));
            Assert.That(response.Itens.First().SessionKey, Is.EqualTo(1));
        }

        [Test]
        public async Task GetQualifyingResultBySessionKey_WhenDbDisposed_ReturnsFailureDataResponse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(databaseName: "GetQualifyDb_Disposed").Options;
            var db = new ApiF1DB(options);
            db.Dispose(); // disposing should cause operations to throw
            var dao = new SessionResultQualifyDao(db);

            // Act
            var response = await dao.GetQualifyingResultBySessionKey(1);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.HasSuccess);
            Assert.IsNotNull(response.Exception);
        }

        [Test]
        public async Task SaveQualifyResult_WithValidEntries_AddsToContextAndReturnsSuccessResponse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(databaseName: "SaveQualifyDb").Options;
            await using var db = new ApiF1DB(options);
            var dao = new SessionResultQualifyDao(db);
            var items = new List<SessionResultQualify>
            {
                new SessionResultQualify { SessionKey = 3, MeetingKey = 20, DriverNumber = 11, QualifyingPhase = "Q3", Duration = 10.5 },
                new SessionResultQualify { SessionKey = 3, MeetingKey = 20, DriverNumber = 12, QualifyingPhase = "Q3", Duration = 10.7 }
            };

            // Act
            var response = await dao.SaveQualifyResult(items);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.HasSuccess);
            // AddRangeAsync adds to the change tracker even if SaveChangesAsync is not called
            var tracked = db.ChangeTracker.Entries<SessionResultQualify>().Count();
            Assert.That(tracked, Is.EqualTo(2));
        }

        [Test]
        public async Task SaveQualifyResult_WhenDbDisposed_ReturnsFailureResponse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(databaseName: "SaveQualifyDb_Disposed").Options;
            var db = new ApiF1DB(options);
            db.Dispose();
            var dao = new SessionResultQualifyDao(db);
            var items = new List<SessionResultQualify> { new SessionResultQualify { SessionKey = 4 } };

            // Act
            var response = await dao.SaveQualifyResult(items);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.HasSuccess);
            Assert.IsNotNull(response.Exception);
        }
    }
}
