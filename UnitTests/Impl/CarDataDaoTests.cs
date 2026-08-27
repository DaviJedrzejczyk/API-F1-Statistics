using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dao.Impl;
using Dao;
using Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shared.Responses;

namespace UnitTests.Impl
{
    [TestFixture]
    public class CarDataDaoTests
    {
        [Test]
        public void Constructor_WithValidDb_DoesNotThrow()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            // Act / Assert
            Assert.DoesNotThrow(() => new CarDataDao(new ApiF1DB(options)));
        }

        [Test]
        public async Task GetHighSpeedSessionDatabase_WithMatchingItems_ReturnsSuccessDataResponse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            await using var db = new ApiF1DB(options);

            // Seed data
            var matching1 = new CarData { SessionKey = 1, Speed = 150, DriverNumber = 7 };
            var matching2 = new CarData { SessionKey = 1, Speed = 160, DriverNumber = 11 };
            var other = new CarData { SessionKey = 2, Speed = 200, DriverNumber = 5 };
            await db.AddRangeAsync(new[] { matching1, matching2, other });
            await db.SaveChangesAsync();

            var dao = new CarDataDao(db);

            // Act
            var result = await dao.GetHighSpeedSessionDatabase(1, 140);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.That(result.Itens.Count, Is.EqualTo(2));
            Assert.That(result.Itens.Any(i => i.DriverNumber == 7));
            Assert.That(result.Itens.Any(i => i.DriverNumber == 11));
        }

        [Test]
        public async Task SaveCarDatas_WhenAddRangeThrows_ReturnsFailureResponse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var mockDb = new Mock<ApiF1DB>(options);

            // Setup AddRangeAsync to throw when called
            mockDb.Setup(d => d.AddRangeAsync(It.IsAny<IEnumerable<object>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var dao = new CarDataDao(mockDb.Object);
            var data = new List<CarData> { new CarData() };

            // Act
            var result = await dao.SaveCarDatas(data);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.That(result.Exception, Is.InstanceOf<InvalidOperationException>());
            Assert.That(result.Exception.Message, Is.EqualTo("boom"));
        }
    }
}
