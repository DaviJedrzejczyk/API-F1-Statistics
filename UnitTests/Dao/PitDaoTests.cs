using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Entities.Class;
using Dao;
using Dao.Impl;
using Shared.Responses;

namespace UnitTests.Dao
{
    [TestFixture]
    public class PitDaoTests
    {
        [Test]
        public async Task Constructor_WithValidDb_AllowsGettingEmptyList()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApiF1DB(options);
            var dao = new PitDao(context);

            // Act
            var response = await dao.GetAllPitsBySessionKey(123);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.HasSuccess);
            Assert.IsNotNull(response.Itens);
            Assert.IsEmpty(response.Itens);
        }

        [Test]
        public async Task GetAllPitsBySessionKey_WithExistingSessionKey_ReturnsPits()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var pit = new Pit { SessionKey = 42, DriverNumber = 7, Date = DateTime.UtcNow };

            await using (var context = new ApiF1DB(options))
            {
                context.Pits.Add(pit);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApiF1DB(options))
            {
                var dao = new PitDao(context);

                // Act
                var response = await dao.GetAllPitsBySessionKey(42);

                // Assert
                Assert.IsNotNull(response);
                Assert.IsTrue(response.HasSuccess);
                Assert.IsNotNull(response.Itens);
                Assert.That(response.Itens.Count, Is.EqualTo(1));
                Assert.That(response.Itens[0].DriverNumber, Is.EqualTo(7));
                Assert.That(response.Itens[0].SessionKey, Is.EqualTo(42));
            }
        }

        [Test]
        public async Task GetAllPitsBySessionKey_WhenDbIsNull_ReturnsFailure()
        {
            // Arrange
            var dao = new PitDao(null!);

            // Act
            var response = await dao.GetAllPitsBySessionKey(1);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.HasSuccess);
            Assert.IsNotNull(response.Exception);
            Assert.IsInstanceOf<NullReferenceException>(response.Exception);
        }

        [Test]
        public async Task SavePits_WithValidDb_ReturnsSuccess()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApiF1DB(options);
            var dao = new PitDao(context);
            var pits = new List<Pit>
            {
                new Pit { SessionKey = 5, DriverNumber = 99 }
            };

            // Act
            var response = await dao.SavePits(pits);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.HasSuccess);
            Assert.That(response.Message, Is.EqualTo("Success to insert all pits."));
        }

        [Test]
        public async Task SavePits_WhenDbIsNull_ReturnsFailure()
        {
            // Arrange
            var dao = new PitDao(null!);
            var pits = new List<Pit> { new Pit { SessionKey = 1 } };

            // Act
            var response = await dao.SavePits(pits);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.HasSuccess);
            Assert.IsNotNull(response.Exception);
            Assert.IsInstanceOf<NullReferenceException>(response.Exception);
        }
    }
}
