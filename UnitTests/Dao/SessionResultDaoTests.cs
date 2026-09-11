using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dao.Impl;
using Dao;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shared.Responses;
using Entities.Class;

namespace UnitTests.Dao
{
    [TestFixture]
    public class SessionResultDaoTests
    {
        [Test]
        public void Constructor_WithValidDb_DoesNotThrow()
        {
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            Assert.DoesNotThrow(() => new SessionResultDao(new ApiF1DB(options)));
        }

        [Test]
        public async Task Constructor_WithMockDb_SaveSessionResults_UsesProvidedDb()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var mockDb = new Mock<ApiF1DB>(options);

            mockDb.Setup(d => d.AddRangeAsync(It.IsAny<IEnumerable<object>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var dao = new SessionResultDao(mockDb.Object);
            var items = new List<SessionResult> { new SessionResult { SessionKey = 10 } };

            // Act
            var result = await dao.SaveSessionResults(items);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            mockDb.Verify(d => d.AddRangeAsync(It.Is<IEnumerable<object>>(e => e.Cast<SessionResult>().SequenceEqual(items)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetSessionResultsBySesssionKey_WithMatchingItems_ReturnsSuccessDataResponse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            await using var db = new ApiF1DB(options);

            var item1 = new SessionResult { SessionKey = 1, DriverNumber = 7 };
            var item2 = new SessionResult { SessionKey = 2, DriverNumber = 8 };
            await db.AddRangeAsync(new[] { item1, item2 });
            await db.SaveChangesAsync();

            var dao = new SessionResultDao(db);

            // Act
            var response = await dao.GetSessionResultsBySesssionKey(1);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.HasSuccess);
            Assert.IsNotNull(response.Itens);
            Assert.That(response.Itens.Count, Is.EqualTo(1));
            Assert.That(response.Itens[0].DriverNumber, Is.EqualTo(7));
        }

        [Test]
        public async Task GetSessionResultsBySesssionKey_WhenDbThrows_ReturnsFailureDataResponse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            await using var db = new ApiF1DB(options);

            // Force a null DbSet to make the DAO throw a NullReferenceException internally
            db.SessionResults = null!;

            var dao = new SessionResultDao(db);

            // Act
            var response = await dao.GetSessionResultsBySesssionKey(1);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.HasSuccess);
            Assert.IsNotNull(response.Exception);
            Assert.IsInstanceOf<ArgumentNullException>(response.Exception);
        }

        [Test]
        public async Task SaveSessionResults_Success_ReturnsSuccessResponse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var mockDb = new Mock<ApiF1DB>(options);
            mockDb.Setup(d => d.AddRangeAsync(It.IsAny<IEnumerable<object>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var dao = new SessionResultDao(mockDb.Object);
            var items = new List<SessionResult> { new SessionResult { SessionKey = 5 } };

            // Act
            var response = await dao.SaveSessionResults(items);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.HasSuccess);
            Assert.That(response.Message, Is.EqualTo("The session results has been saved."));
            mockDb.Verify(d => d.AddRangeAsync(It.IsAny<IEnumerable<object>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SaveSessionResults_AddRangeThrows_ReturnsFailureResponse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var mockDb = new Mock<ApiF1DB>(options);
            mockDb.Setup(d => d.AddRangeAsync(It.IsAny<IEnumerable<object>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var dao = new SessionResultDao(mockDb.Object);

            // Act
            var response = await dao.SaveSessionResults(new List<SessionResult>());

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.HasSuccess);
            Assert.IsNotNull(response.Exception);
            Assert.IsInstanceOf<InvalidOperationException>(response.Exception);
        }
    }
}
